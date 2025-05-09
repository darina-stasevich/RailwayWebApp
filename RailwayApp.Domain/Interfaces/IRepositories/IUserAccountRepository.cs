using RailwayApp.Domain.Entities;

namespace RailwayApp.Domain.Interfaces.IRepositories;

public interface IUserAccountRepository : IGenericRepository<UserAccount, Guid>
{
    Task<UserAccount?> GetByEmailAsync(string email);
    Task<bool> UpdateAsync(Guid id, UserAccount user);
}