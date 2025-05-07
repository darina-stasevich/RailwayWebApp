using RailwayApp.Domain.Entities;

namespace RailwayApp.Domain.Interfaces.IRepositories;

public interface IUserAccountRepository : IGlobalRepository<UserAccount, Guid>
{
    Task<UserAccount?> GetByEmailAsync(string email);
    Task<bool> UpdateAsync(Guid id, UserAccount user);
}