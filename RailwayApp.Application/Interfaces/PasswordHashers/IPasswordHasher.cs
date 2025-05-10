namespace RailwayApp.Domain.Interfaces.IServices;

public interface IPasswordHasher
{
    public Task<string> HashPassword(string password);

    public bool VerifyHashedPassword(string hashedPassword, string providedPassword);
}