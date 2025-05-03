using RailwayApp.Domain.Entities;

namespace RailwayApp.Domain.Interfaces.IRepositories;

public interface IUserAccountRepository
{
    Task DeleteAllAsync();
    Task<string> CreateAsync(UserAccount user);
    Task<UserAccount?> GetByEmailAsync(string email);

}