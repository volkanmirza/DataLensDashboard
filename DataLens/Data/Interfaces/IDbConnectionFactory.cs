using System.Data;
using MongoDB.Driver;

namespace DataLens.Data.Interfaces
{
    public interface IDbConnectionFactory
    {
        IDbConnection CreateConnection();
        IMongoDatabase CreateMongoDatabase();
        string GetDatabaseType();
        string ConnectionString { get; }
    }
}