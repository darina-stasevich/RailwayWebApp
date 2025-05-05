using RailwayApp.Application.Models;
using RailwayApp.Domain;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces.IRepositories;
using RailwayApp.Domain.Interfaces.IServices;
using RailwayApp.Domain.Statuses;

namespace RailwayApp.Application.Services;

public class UserAccountService(IPasswordHasher passwordHasher,
    IUserAccountRepository userAccountRepository) : IUserAccountService
{
    private UserAccount MapUserAccount(CreateUserAccountRequest request)
    {
        var userAccount = new UserAccount
        {
            Email = request.Email,
            Surname = request.Surname,
            Name = request.Name,
            SecondName = request.SecondName,
            PhoneNumber = request.PhoneNumber,
            BirthDate = request.BirthDate,
            Gender = request.Gender,
            Status = UserAccountStatus.Active,
            StatusChangedDate = DateTime.Now,
        };
        return userAccount;
    }
    
    private void MapUpdateRequestToUserAccount(UpdateUserAccountRequest request, UserAccount userAccount)
    {
        userAccount.Surname = request.Surname;
        userAccount.Name = request.Name;
        userAccount.SecondName = request.SecondName;
        userAccount.PhoneNumber = request.PhoneNumber;
        userAccount.BirthDate = request.BirthDate.ToUniversalTime();
        userAccount.Gender = request.Gender;
    }
    
    public async Task<Guid> CreateUserAccountAsync(CreateUserAccountRequest request)
    {
        var userAccountCheck = await userAccountRepository.GetByEmailAsync(request.Email);
        if (userAccountCheck != null)
            throw new UserServiceEmailAlreadyExistsException(request.Email);

        var userAccount = MapUserAccount(request);

        var passwordHash = await passwordHasher.HashPassword(request.Password);
        userAccount.HashedPassword = passwordHash;
        
        return await userAccountRepository.CreateAsync(userAccount);
    }

    public async Task<Guid> UpdateUserAccountAsync(Guid userAccountId, UpdateUserAccountRequest request)
    {
        var userAccount = await userAccountRepository.GetByIdAsync(userAccountId);
        if (userAccount == null)
            throw new UserServiceUserNotFoundException(userAccountId);
        
        if(userAccount.Status == UserAccountStatus.Blocked)
            throw new UserServiceUserBlockedException(userAccountId);
        
        MapUpdateRequestToUserAccount(request, userAccount);

        var resultUpdate = await userAccountRepository.UpdateAsync(userAccountId, userAccount);
        if (resultUpdate == false)
            throw new Exception("Updating user account failed");
        
        return userAccountId;
    }

    public async Task<Guid> UpdateUserPasswordAsync(Guid userAccountId, ChangePasswordRequest request)
    {
        var userAccount = await userAccountRepository.GetByIdAsync(userAccountId);
        if (userAccount == null)
            throw new UserServiceUserNotFoundException(userAccountId);
        
        if(userAccount.Status == UserAccountStatus.Blocked)
            throw new UserServiceUserBlockedException(userAccountId);

        var oldPasswordHash = userAccount.HashedPassword;
        if(oldPasswordHash != userAccount.HashedPassword)
            throw new UserServiceInvalidPasswordException(userAccount.Email);
        
        var passwordHash = await passwordHasher.HashPassword(request.NewPassword);
        userAccount.HashedPassword = passwordHash;
        var resultUpdate = await userAccountRepository.UpdateAsync(userAccountId, userAccount);
        
        if (resultUpdate == false)
            throw new Exception("Updating user account failed");

        return userAccountId;
    }

    public async Task<Guid> DeleteUserAccountAsync(Guid userAccountId)
    {
        var userAccount = await userAccountRepository.GetByIdAsync(userAccountId);
        if (userAccount == null)
            throw new UserServiceUserNotFoundException(userAccountId);
        
        if(userAccount.Status == UserAccountStatus.Blocked)
            throw new UserServiceUserBlockedException(userAccountId);

        var resultDelete = await userAccountRepository.DeleteAsync(userAccountId);
        if (resultDelete == false)
            throw new Exception("Deleting user account failed");

        return userAccountId;
    }
}