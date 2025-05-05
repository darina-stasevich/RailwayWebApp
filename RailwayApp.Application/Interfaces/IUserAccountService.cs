using RailwayApp.Application.Models;
using RailwayApp.Domain.Entities;

namespace RailwayApp.Domain.Interfaces.IServices;

public interface IUserAccountService
{
    Task<Guid> CreateUserAccountAsync(CreateUserAccountRequest request);
    Task<Guid> UpdateUserAccountAsync(Guid userAccountId, UpdateUserAccountRequest request);
    Task<Guid> UpdateUserPasswordAsync(Guid userAccountId, ChangePasswordRequest request);
    Task<Guid> DeleteUserAccountAsync(Guid userAccountId);
    
    
}