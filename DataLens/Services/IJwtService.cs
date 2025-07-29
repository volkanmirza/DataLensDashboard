using DataLens.Models;

namespace DataLens.Services
{
    public interface IJwtService
    {
        string GenerateToken(User user);
        string? ValidateToken(string token);
        bool IsTokenValid(string token);
    }
}