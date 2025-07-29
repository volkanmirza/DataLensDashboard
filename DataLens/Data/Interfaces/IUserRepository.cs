using DataLens.Models;

namespace DataLens.Data.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> GetByEmailAsync(string email);
        Task<IEnumerable<User>> GetByRoleAsync(string role);
        Task<IEnumerable<User>> GetActiveUsersAsync();
        Task<bool> IsUsernameExistsAsync(string username);
        Task<bool> IsEmailExistsAsync(string email);
        Task<bool> UpdatePasswordAsync(string userId, string passwordHash);
        Task<bool> UpdateLastLoginAsync(string userId);
    }
}