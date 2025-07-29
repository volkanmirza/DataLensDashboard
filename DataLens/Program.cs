using DataLens.Data;
using DataLens.Data.Interfaces;
using DataLens.Data.SqlServer;
using DataLens.Data.MongoDB;
using MongoDB.Driver;
using DataLens.Services;
using DataLens.Services.Interfaces;
using DataLens.Middleware;
using DataLens.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using DevExpress.AspNetCore;
using DevExpress.DashboardAspNetCore;
using DevExpress.DashboardCommon;
using DevExpress.DashboardWeb;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// DevExpress Dashboard Configuration
builder.Services.AddDevExpressControls();

// Database Configuration
builder.Services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();

// Unit of Work Factory Registration
builder.Services.AddScoped<IUnitOfWorkFactory, UnitOfWorkFactory>();

// Repository Registration based on Database Type
var databaseType = builder.Configuration["DatabaseSettings:DatabaseType"] ?? "SqlServer";

switch (databaseType)
{
    case "SqlServer":
    case "PostgreSQL":
        builder.Services.AddScoped<IUserRepository, SqlUserRepository>();
        builder.Services.AddScoped<IUserGroupMembershipRepository, SqlUserGroupMembershipRepository>();
        builder.Services.AddScoped<IDashboardRepository>(provider =>
        {
            var connectionFactory = provider.GetRequiredService<IDbConnectionFactory>();
            return new SqlDashboardRepository(connectionFactory.ConnectionString);
        });
        builder.Services.AddScoped<IDashboardPermissionRepository>(provider =>
        {
            var connectionFactory = provider.GetRequiredService<IDbConnectionFactory>();
            return new SqlDashboardPermissionRepository(connectionFactory.ConnectionString);
        });
        break;
    case "MongoDB":
        builder.Services.AddSingleton<IMongoDatabase>(provider =>
        {
            var connectionFactory = provider.GetRequiredService<IDbConnectionFactory>();
            return connectionFactory.CreateMongoDatabase();
        });
        builder.Services.AddScoped<IUserRepository, MongoUserRepository>();
        builder.Services.AddScoped<IUserGroupRepository, MongoUserGroupRepository>();
        builder.Services.AddScoped<IUserGroupMemberRepository, MongoUserGroupMemberRepository>();
        builder.Services.AddScoped<IUserGroupMembershipRepository, MongoUserGroupMembershipRepository>();
        builder.Services.AddScoped<IDashboardRepository, MongoDashboardRepository>();
        builder.Services.AddScoped<IDashboardPermissionRepository, MongoDashboardPermissionRepository>();
        builder.Services.AddScoped<INotificationRepository, MongoNotificationRepository>();
        builder.Services.AddScoped<IUserSettingsRepository, MongoUserSettingsRepository>();
        builder.Services.AddScoped<IUnitOfWork, MongoUnitOfWork>();
        builder.Services.AddScoped<IMongoDbInitializer, MongoDbInitializer>();
        break;
    default:
        throw new NotSupportedException($"Database type '{databaseType}' is not supported");
}

// JWT Service Registration
builder.Services.AddScoped<IJwtService, JwtService>();

// Business Services Registration
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserGroupService, UserGroupService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

// JWT Authentication Configuration
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new ArgumentNullException("JwtSettings:SecretKey");
var issuer = jwtSettings["Issuer"] ?? throw new ArgumentNullException("JwtSettings:Issuer");
var audience = jwtSettings["Audience"] ?? throw new ArgumentNullException("JwtSettings:Audience");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey)),
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("DesignerOrAdmin", policy => policy.RequireRole("Designer", "Admin"));
    options.AddPolicy("AllUsers", policy => policy.RequireRole("Viewer", "Designer", "Admin"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// DevExpress Controls
app.UseDevExpressControls();

app.UseRouting();

// DevExpress Dashboard endpoint
app.MapDashboardRoute("api/dashboard", "DefaultDashboard");

// JWT Cookie Middleware - must be before UseAuthentication
app.UseJwtCookie();

app.UseAuthentication();
app.UseAuthorization();

// Area routing
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// MongoDB Database Initialization
if (databaseType == "MongoDB")
{
    using (var scope = app.Services.CreateScope())
    {
        var mongoInitializer = scope.ServiceProvider.GetRequiredService<IMongoDbInitializer>();
        if (!await mongoInitializer.IsDatabaseInitializedAsync())
        {
            await mongoInitializer.InitializeAsync();
            await mongoInitializer.SeedDataAsync();
        }
    }
}

app.Run();
