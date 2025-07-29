using DataLens.Data.Interfaces;
using DataLens.Data.SqlServer;
using DataLens.Data.MongoDB;

namespace DataLens.Data
{
    public class UnitOfWorkFactory : IUnitOfWorkFactory
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly string _databaseType;

        public UnitOfWorkFactory(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
            _databaseType = connectionFactory.GetDatabaseType();
        }

        public IUnitOfWork CreateUnitOfWork()
        {
            return _databaseType switch
            {
                "SqlServer" or "PostgreSQL" => new SqlUnitOfWork(_connectionFactory),
                "MongoDB" => new MongoUnitOfWork(_connectionFactory),
                _ => throw new NotSupportedException($"Database type '{_databaseType}' is not supported for Unit of Work")
            };
        }
    }
}