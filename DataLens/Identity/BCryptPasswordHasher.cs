using Microsoft.AspNetCore.Identity;
using DataLens.Models;

namespace DataLens.Identity
{
    public class BCryptPasswordHasher : IPasswordHasher<User>
    {
        public string HashPassword(User user, string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt());
        }

        public PasswordVerificationResult VerifyHashedPassword(User user, string hashedPassword, string providedPassword)
        {
            try
            {
                bool isValid = BCrypt.Net.BCrypt.Verify(providedPassword, hashedPassword);
                return isValid ? PasswordVerificationResult.Success : PasswordVerificationResult.Failed;
            }
            catch
            {
                return PasswordVerificationResult.Failed;
            }
        }
    }
}