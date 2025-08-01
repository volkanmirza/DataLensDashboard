using Microsoft.AspNetCore.Identity;
using MongoDB.Driver;
using System.Security.Claims;

namespace DataLens.Identity
{
    public class MongoRole : IdentityRole
    {
        public MongoRole() { }
        public MongoRole(string roleName) : base(roleName) { }
    }

    public class MongoRoleStore : IRoleStore<MongoRole>, IRoleClaimStore<MongoRole>
    {
        private readonly IMongoCollection<MongoRole> _roles;
        private bool _disposed = false;

        public MongoRoleStore(IMongoDatabase database)
        {
            _roles = database.GetCollection<MongoRole>("Roles");
        }

        #region IRoleStore<MongoRole>
        public async Task<IdentityResult> CreateAsync(MongoRole role, CancellationToken cancellationToken)
        {
            try
            {
                role.Id = MongoDB.Bson.ObjectId.GenerateNewId().ToString();
                await _roles.InsertOneAsync(role, cancellationToken: cancellationToken);
                return IdentityResult.Success;
            }
            catch (Exception ex)
            {
                return IdentityResult.Failed(new IdentityError { Description = ex.Message });
            }
        }

        public async Task<IdentityResult> DeleteAsync(MongoRole role, CancellationToken cancellationToken)
        {
            try
            {
                var filter = Builders<MongoRole>.Filter.Eq(r => r.Id, role.Id);
                await _roles.DeleteOneAsync(filter, cancellationToken);
                return IdentityResult.Success;
            }
            catch (Exception ex)
            {
                return IdentityResult.Failed(new IdentityError { Description = ex.Message });
            }
        }

        public async Task<MongoRole?> FindByIdAsync(string roleId, CancellationToken cancellationToken)
        {
            var filter = Builders<MongoRole>.Filter.Eq(r => r.Id, roleId);
            return await _roles.Find(filter).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<MongoRole?> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
        {
            var filter = Builders<MongoRole>.Filter.Eq(r => r.NormalizedName, normalizedRoleName);
            return await _roles.Find(filter).FirstOrDefaultAsync(cancellationToken);
        }

        public Task<string?> GetNormalizedRoleNameAsync(MongoRole role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role.NormalizedName);
        }

        public Task<string> GetRoleIdAsync(MongoRole role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role.Id);
        }

        public Task<string?> GetRoleNameAsync(MongoRole role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role.Name);
        }

        public Task SetNormalizedRoleNameAsync(MongoRole role, string? normalizedName, CancellationToken cancellationToken)
        {
            role.NormalizedName = normalizedName;
            return Task.CompletedTask;
        }

        public Task SetRoleNameAsync(MongoRole role, string? roleName, CancellationToken cancellationToken)
        {
            role.Name = roleName;
            return Task.CompletedTask;
        }

        public async Task<IdentityResult> UpdateAsync(MongoRole role, CancellationToken cancellationToken)
        {
            try
            {
                var filter = Builders<MongoRole>.Filter.Eq(r => r.Id, role.Id);
                await _roles.ReplaceOneAsync(filter, role, cancellationToken: cancellationToken);
                return IdentityResult.Success;
            }
            catch (Exception ex)
            {
                return IdentityResult.Failed(new IdentityError { Description = ex.Message });
            }
        }
        #endregion

        #region IRoleClaimStore<MongoRole>
        public Task AddClaimAsync(MongoRole role, Claim claim, CancellationToken cancellationToken = default)
        {
            // MongoDB Role model doesn't have Claims collection, implement if needed
            return Task.CompletedTask;
        }

        public Task<IList<Claim>> GetClaimsAsync(MongoRole role, CancellationToken cancellationToken = default)
        {
            // Return empty claims list for now
            return Task.FromResult<IList<Claim>>(new List<Claim>());
        }

        public Task RemoveClaimAsync(MongoRole role, Claim claim, CancellationToken cancellationToken = default)
        {
            // Implement based on your claim storage strategy
            return Task.CompletedTask;
        }
        #endregion

        public void Dispose()
        {
            _disposed = true;
        }
    }
}