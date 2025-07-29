using DevExpress.DashboardAspNetCore;
using DevExpress.DashboardCommon;
using DevExpress.DashboardWeb;
using DevExpress.DataAccess.ConnectionParameters;
using DevExpress.DataAccess.Web;
using DevExpress.DataAccess.Sql;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DataLens.Data.Interfaces;
using DataLens.Models;
using System.Xml.Linq;

namespace DataLens.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardApiController : ControllerBase
    {
        private readonly IDashboardRepository _dashboardRepository;
        private readonly IConfiguration _configuration;

        public DashboardApiController(IDashboardRepository dashboardRepository, IConfiguration configuration)
        {
            _dashboardRepository = dashboardRepository;
            _configuration = configuration;
        }

        [HttpPost("configure")]
        [Authorize(Policy = "DesignerOrAdmin")]
        public IActionResult ConfigureDashboard()
        {
            var dashboardConfigurator = new DashboardConfigurator();
            
            // Dashboard storage configuration - using database storage
            dashboardConfigurator.SetDashboardStorage(new DatabaseDashboardStorage(_dashboardRepository));
            
            // Data source configuration
            dashboardConfigurator.SetDataSourceStorage(new DataSourceInMemoryStorage());
            
            // Connection string provider
            dashboardConfigurator.SetConnectionStringsProvider(new DashboardConnectionStringsProvider(_configuration));
            
            return Ok();
        }
    }

    public class DashboardConnectionStringsProvider : IDataSourceWizardConnectionStringsProvider
    {
        private readonly IConfiguration _configuration;

        public DashboardConnectionStringsProvider(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Dictionary<string, string> GetConnectionDescriptions()
        {
            return new Dictionary<string, string>
            {
                { "SqlServerConnection", "SQL Server Database" },
                { "PostgreSQLConnection", "PostgreSQL Database" },
                { "MongoDBConnection", "MongoDB Database" }
            };
        }

        public DataConnectionParametersBase? GetDataConnectionParameters(string name)
        {
            var databaseType = _configuration["DatabaseSettings:DatabaseType"];
            
            return databaseType?.ToLower() switch
            {
                "sqlserver" => new MsSqlConnectionParameters()
                {
                    ServerName = "(localdb)\\mssqllocaldb",
                    DatabaseName = "DataLensDb",
                    AuthorizationType = DevExpress.DataAccess.ConnectionParameters.MsSqlAuthorizationType.Windows
                },
                "postgresql" => new PostgreSqlConnectionParameters("localhost", "DataLensDb", "postgres", "password"),
                _ => null
            };
        }
    }

    public class DatabaseDashboardStorage : IDashboardStorage
    {
        private readonly IDashboardRepository _dashboardRepository;

        public DatabaseDashboardStorage(IDashboardRepository dashboardRepository)
        {
            _dashboardRepository = dashboardRepository;
        }

        public XDocument LoadDashboard(string dashboardID)
        {
            var dashboard = _dashboardRepository.GetByIdAsync(dashboardID).Result;
            if (dashboard?.DashboardData != null)
            {
                return XDocument.Parse(dashboard.DashboardData);
            }
            throw new ArgumentException($"Dashboard with ID {dashboardID} not found.");
        }

        public void SaveDashboard(string dashboardID, XDocument dashboard)
        {
            var existingDashboard = _dashboardRepository.GetByIdAsync(dashboardID).Result;
            if (existingDashboard != null)
            {
                existingDashboard.DashboardData = dashboard.ToString();
                existingDashboard.LastModifiedDate = DateTime.UtcNow;
                _dashboardRepository.UpdateAsync(existingDashboard).Wait();
            }
        }

        public string AddDashboard(XDocument dashboard, string dashboardName)
        {
            var newDashboard = new DataLens.Models.Dashboard
            {
                Name = dashboardName,
                Title = dashboardName,
                DashboardData = dashboard.ToString(),
                CreatedDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow,
                IsActive = true,
                CreatedBy = "System"
            };
            
            var result = _dashboardRepository.AddAsync(newDashboard).Result;
            return result;
        }

        public IEnumerable<DashboardInfo> GetAvailableDashboardsInfo()
        {
            var dashboards = _dashboardRepository.GetAllAsync().Result;
            return dashboards.Where(d => d.IsActive).Select(d => new DashboardInfo
            {
                ID = d.Id,
                Name = d.Name
            });
        }
    }
}