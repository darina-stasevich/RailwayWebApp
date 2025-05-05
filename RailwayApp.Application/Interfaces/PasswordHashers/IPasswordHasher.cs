namespace RailwayApp.Domain.Interfaces.IServices;

public interface IPasswordHasher
{
    public Task<string> HashPassword(string password);

    public Task<bool> VerifyHashedPassword(string hashedPassword, string providedPassword);
}