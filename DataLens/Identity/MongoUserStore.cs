using Microsoft.AspNetCore.Identity;
using MongoDB.Driver;
using DataLens.Models;
using System.Security.Claims;

namespace DataLens.Identity
{
    public class MongoUserStore : IUserStore<User>, IUserPasswordStore<User>, IUserEmailStore<User>, 
                                  IUserRoleStore<User>, IUserClaimStore<User>, IUserLoginStore<User>, 
                                  IUserSecurityStampStore<User>, IUserLockoutStore<User>
    {
        private readonly IMongoCollection<User> _users;
        private bool _disposed = false;

        public MongoUserStore(IMongoDatabase database)
        {
            _users = database.GetCollection<User>("Users");
        }

        #region IUserStore<User>
        public async Task<IdentityResult> CreateAsync(User user, CancellationToken cancellationToken)
        {
            try
            {
                user.Id = MongoDB.Bson.ObjectId.GenerateNewId().ToString();
                user.CreatedDate = DateTime.UtcNow;
                user.IsActive = true;
                await _users.InsertOneAsync(user, cancellationToken: cancellationToken);
                return IdentityResult.Success;
            }
            catch (Exception ex)
            {
                return IdentityResult.Failed(new IdentityError { Description = ex.Message });
            }
        }

        public async Task<IdentityResult> DeleteAsync(User user, CancellationToken cancellationToken)
        {
            try
            {
                var filter = Builders<User>.Filter.Eq(u => u.Id, user.Id);
                await _users.DeleteOneAsync(filter, cancellationToken);
                return IdentityResult.Success;
            }
            catch (Exception ex)
            {
                return IdentityResult.Failed(new IdentityError { Description = ex.Message });
            }
        }

        public async Task<User?> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
            return await _users.Find(filter).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<User?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            var filter = Builders<User>.Filter.Eq(u => u.NormalizedUserName, normalizedUserName);
            return await _users.Find(filter).FirstOrDefaultAsync(cancellationToken);
        }

        public Task<string?> GetNormalizedUserNameAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.NormalizedUserName);
        }

        public Task<string> GetUserIdAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Id);
        }

        public Task<string?> GetUserNameAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.UserName);
        }

        public Task SetNormalizedUserNameAsync(User user, string? normalizedName, CancellationToken cancellationToken)
        {
            user.NormalizedUserName = normalizedName;
            return Task.CompletedTask;
        }

        public Task SetUserNameAsync(User user, string? userName, CancellationToken cancellationToken)
        {
            user.UserName = userName;
            return Task.CompletedTask;
        }

        public async Task<IdentityResult> UpdateAsync(User user, CancellationToken cancellationToken)
        {
            try
            {
                var filter = Builders<User>.Filter.Eq(u => u.Id, user.Id);
                await _users.ReplaceOneAsync(filter, user, cancellationToken: cancellationToken);
                return IdentityResult.Success;
            }
            catch (Exception ex)
            {
                return IdentityResult.Failed(new IdentityError { Description = ex.Message });
            }
        }
        #endregion

        #region IUserPasswordStore<User>
        public Task<string?> GetPasswordHashAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.PasswordHash);
        }

        public Task<bool> HasPasswordAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(!string.IsNullOrEmpty(user.PasswordHash));
        }

        public Task SetPasswordHashAsync(User user, string? passwordHash, CancellationToken cancellationToken)
        {
            user.PasswordHash = passwordHash;
            return Task.CompletedTask;
        }
        #endregion

        #region IUserEmailStore<User>
        public async Task<User?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        {
            var filter = Builders<User>.Filter.Eq(u => u.NormalizedEmail, normalizedEmail);
            return await _users.Find(filter).FirstOrDefaultAsync(cancellationToken);
        }

        public Task<string?> GetEmailAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Email);
        }

        public Task<bool> GetEmailConfirmedAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.EmailConfirmed);
        }

        public Task<string?> GetNormalizedEmailAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.NormalizedEmail);
        }

        public Task SetEmailAsync(User user, string? email, CancellationToken cancellationToken)
        {
            user.Email = email;
            return Task.CompletedTask;
        }

        public Task SetEmailConfirmedAsync(User user, bool confirmed, CancellationToken cancellationToken)
        {
            user.EmailConfirmed = confirmed;
            return Task.CompletedTask;
        }

        public Task SetNormalizedEmailAsync(User user, string? normalizedEmail, CancellationToken cancellationToken)
        {
            user.NormalizedEmail = normalizedEmail;
            return Task.CompletedTask;
        }
        #endregion

        #region IUserRoleStore<User>
        public Task AddToRoleAsync(User user, string roleName, CancellationToken cancellationToken)
        {
            user.Role = roleName;
            return Task.CompletedTask;
        }

        public Task<IList<string>> GetRolesAsync(User user, CancellationToken cancellationToken)
        {
            IList<string> roles = new List<string>();
            if (!string.IsNullOrEmpty(user.Role))
            {
                roles.Add(user.Role);
            }
            return Task.FromResult(roles);
        }

        public async Task<IList<User>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Role, roleName);
            var users = await _users.Find(filter).ToListAsync(cancellationToken);
            return users;
        }

        public Task<bool> IsInRoleAsync(User user, string roleName, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Role == roleName);
        }

        public Task RemoveFromRoleAsync(User user, string roleName, CancellationToken cancellationToken)
        {
            if (user.Role == roleName)
            {
                user.Role = string.Empty;
            }
            return Task.CompletedTask;
        }
        #endregion

        #region IUserClaimStore<User>
        public Task AddClaimsAsync(User user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
        {
            // MongoDB User model doesn't have Claims collection, implement if needed
            return Task.CompletedTask;
        }

        public Task<IList<Claim>> GetClaimsAsync(User user, CancellationToken cancellationToken)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName ?? ""),
                new Claim(ClaimTypes.Email, user.Email ?? ""),
                new Claim(ClaimTypes.Role, user.Role ?? "")
            };

            if (!string.IsNullOrEmpty(user.FirstName))
                claims.Add(new Claim("FirstName", user.FirstName));
            
            if (!string.IsNullOrEmpty(user.LastName))
                claims.Add(new Claim("LastName", user.LastName));

            return Task.FromResult<IList<Claim>>(claims);
        }

        public Task<IList<User>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken)
        {
            // Implement based on your claim storage strategy
            return Task.FromResult<IList<User>>(new List<User>());
        }

        public Task RemoveClaimsAsync(User user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
        {
            // Implement based on your claim storage strategy
            return Task.CompletedTask;
        }

        public Task ReplaceClaimAsync(User user, Claim claim, Claim newClaim, CancellationToken cancellationToken)
        {
            // Implement based on your claim storage strategy
            return Task.CompletedTask;
        }
        #endregion

        #region IUserLoginStore<User>
        public Task AddLoginAsync(User user, UserLoginInfo login, CancellationToken cancellationToken)
        {
            // Implement external login support if needed
            return Task.CompletedTask;
        }

        public Task<User?> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            // Implement external login support if needed
            return Task.FromResult<User?>(null);
        }

        public Task<IList<UserLoginInfo>> GetLoginsAsync(User user, CancellationToken cancellationToken)
        {
            // Implement external login support if needed
            return Task.FromResult<IList<UserLoginInfo>>(new List<UserLoginInfo>());
        }

        public Task RemoveLoginAsync(User user, string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            // Implement external login support if needed
            return Task.CompletedTask;
        }
        #endregion

        #region IUserSecurityStampStore<User>
        public Task<string?> GetSecurityStampAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.SecurityStamp);
        }

        public Task SetSecurityStampAsync(User user, string stamp, CancellationToken cancellationToken)
        {
            user.SecurityStamp = stamp;
            return Task.CompletedTask;
        }
        #endregion

        #region IUserLockoutStore<User>
        public Task<int> GetAccessFailedCountAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.AccessFailedCount);
        }

        public Task<bool> GetLockoutEnabledAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.LockoutEnabled);
        }

        public Task<DateTimeOffset?> GetLockoutEndDateAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.LockoutEnd);
        }

        public Task<int> IncrementAccessFailedCountAsync(User user, CancellationToken cancellationToken)
        {
            user.AccessFailedCount++;
            return Task.FromResult(user.AccessFailedCount);
        }

        public Task ResetAccessFailedCountAsync(User user, CancellationToken cancellationToken)
        {
            user.AccessFailedCount = 0;
            return Task.CompletedTask;
        }

        public Task SetLockoutEnabledAsync(User user, bool enabled, CancellationToken cancellationToken)
        {
            user.LockoutEnabled = enabled;
            return Task.CompletedTask;
        }

        public Task SetLockoutEndDateAsync(User user, DateTimeOffset? lockoutEnd, CancellationToken cancellationToken)
        {
            user.LockoutEnd = lockoutEnd;
            return Task.CompletedTask;
        }
        #endregion

        public void Dispose()
        {
            _disposed = true;
        }
    }
}