using DataLens.Data.Interfaces;
using DataLens.Models;
using DataLens.Services.Interfaces;

namespace DataLens.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<User?> GetByIdAsync(string id)
        {
            return await _unitOfWork.Users.GetByIdAsync(id);
        }

        public async Task<User?> GetUserByIdAsync(string id)
        {
            return await _unitOfWork.Users.GetByIdAsync(id);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _unitOfWork.Users.GetByEmailAsync(email);
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _unitOfWork.Users.GetByUsernameAsync(username);
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _unitOfWork.Users.GetAllAsync();
        }

        public async Task<string> CreateUserAsync(User user, string password)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // Hash password
                user.PasswordHash = HashPassword(password);
                user.Id = Guid.NewGuid().ToString();
                user.CreatedDate = DateTime.UtcNow;
                user.IsActive = true;

                // Check if username or email already exists
                if (await _unitOfWork.Users.IsUsernameExistsAsync(user.UserName))
                {
                    throw new InvalidOperationException("Username already exists");
                }

                if (await _unitOfWork.Users.IsEmailExistsAsync(user.Email))
                {
                    throw new InvalidOperationException("Email already exists");
                }

                var userId = await _unitOfWork.Users.AddAsync(user);
                await _unitOfWork.CommitAsync();

                return userId;
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var existingUser = await _unitOfWork.Users.GetByIdAsync(user.Id);
                if (existingUser == null)
                {
                    throw new InvalidOperationException("User not found");
                }

                // Check if username or email already exists for other users
                var userWithSameUsername = await _unitOfWork.Users.GetByUsernameAsync(user.UserName);
                if (userWithSameUsername != null && userWithSameUsername.Id != user.Id)
                {
                    throw new InvalidOperationException("Username already exists");
                }

                var userWithSameEmail = await _unitOfWork.Users.GetByEmailAsync(user.Email);
                if (userWithSameEmail != null && userWithSameEmail.Id != user.Id)
                {
                    throw new InvalidOperationException("Email already exists");
                }

                var result = await _unitOfWork.Users.UpdateAsync(user);
                await _unitOfWork.CommitAsync();

                return result;
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> DeleteUserAsync(string id)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var user = await _unitOfWork.Users.GetByIdAsync(id);
                if (user == null)
                {
                    throw new InvalidOperationException("User not found");
                }

                var result = await _unitOfWork.Users.DeleteAsync(id);
                await _unitOfWork.CommitAsync();

                return result;
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null)
                {
                    throw new InvalidOperationException("User not found");
                }

                // Verify current password
                if (!VerifyPassword(currentPassword, user.PasswordHash))
                {
                    throw new InvalidOperationException("Current password is incorrect");
                }

                // Update password
                var newPasswordHash = HashPassword(newPassword);
                var result = await _unitOfWork.Users.UpdatePasswordAsync(userId, newPasswordHash);
                await _unitOfWork.CommitAsync();

                return result;
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<User?> ValidateUserAsync(string username, string password)
        {
            var user = await _unitOfWork.Users.GetByUsernameAsync(username);
            if (user == null || !user.IsActive)
            {
                return null;
            }

            if (VerifyPassword(password, user.PasswordHash))
            {
                // Update last login
                await _unitOfWork.Users.UpdateLastLoginAsync(user.Id);
                return user;
            }

            return null;
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt());
        }

        private bool VerifyPassword(string password, string hash)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(password, hash);
            }
            catch
            {
                return false;
            }
        }
    }
}

public static class UnitOfWorkExtensions
{
    public static bool IsRelationalDb(this IUnitOfWork unitOfWork)
        => unitOfWork.DatabaseType == "SqlServer" || unitOfWork.DatabaseType == "PostgreSQL";
}