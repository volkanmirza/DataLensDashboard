using DataLens.Models;

namespace DataLens.Services.Interfaces
{
    public interface IUserService
    {
        Task<User?> GetByIdAsync(string id);
        Task<User?> GetUserByIdAsync(string id);
        Task<User?> GetUserByUsernameAsync(string username);
        Task<User?> GetByEmailAsync(string email);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<string> CreateUserAsync(User user, string password);
        Task<bool> UpdateUserAsync(User user);
        Task<bool> DeleteUserAsync(string id);
        Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
        Task<User?> ValidateUserAsync(string username, string password);
    }
}