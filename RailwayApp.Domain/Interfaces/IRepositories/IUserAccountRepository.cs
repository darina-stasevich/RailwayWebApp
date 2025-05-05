using RailwayApp.Domain.Entities;

namespace RailwayApp.Domain.Interfaces.IRepositories;

public interface IUserAccountRepository
{
    Task DeleteAllAsync();
    Task<Guid> CreateAsync(UserAccount user);
    Task<UserAccount?> GetByIdAsync(Guid id);
    Task<UserAccount?> GetByEmailAsync(string email);
    Task<bool> UpdateAsync(Guid id, UserAccount user);
    Task<bool> DeleteAsync(Guid id);
}