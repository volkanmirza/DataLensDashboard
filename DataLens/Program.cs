using DataLens.Data;
using DataLens.Data.Interfaces;
using DataLens.Data.SqlServer;
using DataLens.Data.MongoDB;
using MongoDB.Driver;
using DataLens.Services;
using DataLens.Services.Interfaces;
using DataLens.Repositories;
using DataLens.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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

// Database type configuration
var databaseType = builder.Configuration["DatabaseSettings:DatabaseType"] ?? "SqlServer";

// Entity Framework and Identity Configuration (only for SQL databases)
if (databaseType == "SqlServer" || databaseType == "PostgreSQL")
{
    var connectionString = databaseType == "SqlServer" 
        ? builder.Configuration["DatabaseSettings:ConnectionStrings:SqlServer"]
        : builder.Configuration["DatabaseSettings:ConnectionStrings:PostgreSQL"];
    
    if (string.IsNullOrEmpty(connectionString))
        throw new InvalidOperationException($"Connection string for {databaseType} not found.");

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
    {
        if (databaseType == "SqlServer")
            options.UseSqlServer(connectionString);
        else
            options.UseNpgsql(connectionString);
    });
}

// Identity configuration
if (databaseType == "SqlServer" || databaseType == "PostgreSQL")
{
    builder.Services.AddIdentity<User, IdentityRole>(options =>
    {
        // Password settings
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = true;
        options.Password.RequiredLength = 6;
        options.Password.RequiredUniqueChars = 1;

        // Lockout settings
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = true;

        // User settings
        options.User.AllowedUserNameCharacters =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
        options.User.RequireUniqueEmail = true;

        // Sign in settings
        options.SignIn.RequireConfirmedEmail = false;
        options.SignIn.RequireConfirmedPhoneNumber = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();
}
else if (databaseType == "MongoDB")
{
    // MongoDB Identity configuration
    builder.Services.AddIdentity<User, DataLens.Identity.MongoRole>(options =>
    {
        // Password settings
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = true;
        options.Password.RequiredLength = 6;
        options.Password.RequiredUniqueChars = 1;

        // Lockout settings
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = true;

        // User settings
        options.User.AllowedUserNameCharacters =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
        options.User.RequireUniqueEmail = true;

        // Sign in settings
        options.SignIn.RequireConfirmedEmail = false;
        options.SignIn.RequireConfirmedPhoneNumber = false;
    })
    .AddUserStore<DataLens.Identity.MongoUserStore>()
    .AddRoleStore<DataLens.Identity.MongoRoleStore>()
    .AddDefaultTokenProviders();
}

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(24);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

// Unit of Work Factory Registration
builder.Services.AddScoped<IUnitOfWorkFactory, UnitOfWorkFactory>();

// Repository Registration based on Database Type

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



// Business Services Registration
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserGroupService, UserGroupService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();



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



app.UseAuthentication();
app.UseAuthorization();

// DevExpress Dashboard endpoint - must be after authentication
app.MapDashboardRoute("api/dashboard", "DefaultDashboard");

// Area routing
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

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
