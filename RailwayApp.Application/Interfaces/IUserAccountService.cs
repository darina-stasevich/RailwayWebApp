using RailwayApp.Application.Models;
using RailwayApp.Domain.Entities;

namespace RailwayApp.Domain.Interfaces.IServices;

public interface IUserAccountService
{
    Task<Guid> CreateUserAccountAsync(UserAccountDto userAccountDto);
    Task<Guid> UpdateUserAccountAsync(Guid userAccountId, UserAccountDto userAccountDto);
    Task<Guid> BlockUserAccountAsync(Guid userAccountId);
}