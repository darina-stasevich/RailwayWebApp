using RailwayApp.Domain.Interfaces.IServices;

namespace RailwayApp.Application.Services.PasswordHashers;

public class BCryptPasswordHasher : IPasswordHasher
{
    public async Task<string> HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }
    public async Task<bool> VerifyHashedPassword(string hashedPassword, string providedPassword)
    {
        return BCrypt.Net.BCrypt.Verify(providedPassword, hashedPassword);
    }
}