using PlantaCoreAPI.Application.Interfaces;
using BCrypt.Net;

namespace PlantaCoreAPI.Infrastructure.Services;

public class PasswordHashService : IPasswordHashService
{
    public string Hash(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool Verify(string password, string hash)
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
