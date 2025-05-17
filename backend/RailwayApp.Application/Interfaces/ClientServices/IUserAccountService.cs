using RailwayApp.Application.Models;

namespace RailwayApp.Domain.Interfaces.IServices;

public interface IUserAccountService
{
    Task<Guid> CreateUserAccountAsync(CreateUserAccountRequest request);
    Task<Guid> UpdateUserAccountAsync(Guid userAccountId, UpdateUserAccountRequest request);
    Task<Guid> UpdateUserPasswordAsync(Guid userAccountId, ChangePasswordRequest request);
    Task<Guid> DeleteUserAccountAsync(Guid userAccountId);
    Task<UserAccountDto> GetUserAccount(Guid userAccountId);
}