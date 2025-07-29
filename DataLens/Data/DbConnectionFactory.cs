using System.Data;
using Microsoft.Data.SqlClient;
using Npgsql;
using MongoDB.Driver;
using DataLens.Data.Interfaces;

namespace DataLens.Data
{
    public class DbConnectionFactory : IDbConnectionFactory
    {
        private readonly IConfiguration _configuration;
        private readonly string _databaseType;
        private readonly string _connectionString;
        private readonly string _mongoConnectionString;

        public DbConnectionFactory(IConfiguration configuration)
        {
            _configuration = configuration;
            _databaseType = _configuration["DatabaseSettings:DatabaseType"] ?? "SqlServer";
            
            _connectionString = _databaseType switch
            {
                "SqlServer" => _configuration["DatabaseSettings:ConnectionStrings:SqlServer"] ?? throw new InvalidOperationException("SQL Server connection string not found"),
                "PostgreSQL" => _configuration["DatabaseSettings:ConnectionStrings:PostgreSQL"] ?? throw new InvalidOperationException("PostgreSQL connection string not found"),
                "MongoDB" => _configuration["DatabaseSettings:ConnectionStrings:MongoDB"] ?? throw new InvalidOperationException("MongoDB connection string not found"),
                _ => throw new NotSupportedException($"Database type '{_databaseType}' is not supported")
            };
            
            _mongoConnectionString = _configuration["DatabaseSettings:ConnectionStrings:MongoDB"] ?? "";
        }

        public IDbConnection CreateConnection()
        {
            return _databaseType switch
            {
                "SqlServer" => new SqlConnection(_connectionString),
                "PostgreSQL" => new NpgsqlConnection(_connectionString),
                "MongoDB" => throw new InvalidOperationException("Use CreateMongoDatabase() for MongoDB connections"),
                _ => throw new NotSupportedException($"Database type '{_databaseType}' is not supported")
            };
        }

        public IMongoDatabase CreateMongoDatabase()
        {
            if (_databaseType != "MongoDB")
                throw new InvalidOperationException("CreateMongoDatabase() can only be used with MongoDB");

            var client = new MongoClient(_mongoConnectionString);
            var databaseName = MongoUrl.Create(_mongoConnectionString).DatabaseName ?? "DataLensDb";
            return client.GetDatabase(databaseName);
        }

        public string GetDatabaseType()
        {
            return _databaseType;
        }

        public string ConnectionString => _connectionString;
    }
}