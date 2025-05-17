using RailwayApp.Application.Models;
using RailwayApp.Domain;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces.IRepositories;
using RailwayApp.Domain.Interfaces.IServices;
using RailwayApp.Domain.Statuses;

namespace RailwayApp.Application.Services;

public class UserAccountService(
    IPasswordHasher passwordHasher,
    IUserAccountRepository userAccountRepository) : IUserAccountService
{
    public async Task<Guid> CreateUserAccountAsync(CreateUserAccountRequest request)
    {
        ValidateAge(request.BirthDate);
        var userAccountCheck = await userAccountRepository.GetByEmailAsync(request.Email);
        if (userAccountCheck != null)
            throw new UserAccountEmailAlreadyExistsException(request.Email);

        var userAccount = MapUserAccount(request);

        var passwordHash = await passwordHasher.HashPassword(request.Password);
        userAccount.HashedPassword = passwordHash;

        return await userAccountRepository.AddAsync(userAccount);
    }

    public async Task<UserAccountDto> UpdateUserAccountAsync(Guid userAccountId, UpdateUserAccountRequest request)
    {
        ValidateAge(request.BirthDate);
        var userAccount = await userAccountRepository.GetByIdAsync(userAccountId);
        if (userAccount == null)
            throw new UserAccountUserNotFoundException(userAccountId);

        if (userAccount.Status == UserAccountStatus.Blocked)
            throw new UserAccountUserBlockedException(userAccountId);

        MapUpdateRequestToUserAccount(request, userAccount);

        var resultUpdate = await userAccountRepository.UpdateAsync(userAccountId, userAccount);
        if (resultUpdate == false)
            throw new UserAccountUpdatingFailed(userAccountId);

        return MapUserAccountDto(userAccount);
    }

    public async Task<Guid> UpdateUserPasswordAsync(Guid userAccountId, ChangePasswordRequest request)
    {
        var userAccount = await userAccountRepository.GetByIdAsync(userAccountId);
        if (userAccount == null)
            throw new UserAccountUserNotFoundException(userAccountId);

        if (userAccount.Status == UserAccountStatus.Blocked)
            throw new UserAccountUserBlockedException(userAccountId);

        var oldPasswordHash = userAccount.HashedPassword;
        if (oldPasswordHash != userAccount.HashedPassword)
            throw new UserAccountInvalidPasswordException(userAccount.Email);

        var passwordHash = await passwordHasher.HashPassword(request.NewPassword);
        userAccount.HashedPassword = passwordHash;
        var resultUpdate = await userAccountRepository.UpdateAsync(userAccountId, userAccount);

        if (resultUpdate == false)
            throw new UserAccountUpdatingFailed(userAccountId);

        return userAccountId;
    }

    public async Task<Guid> DeleteUserAccountAsync(Guid userAccountId)
    {
        var userAccount = await userAccountRepository.GetByIdAsync(userAccountId);
        if (userAccount == null)
            throw new UserAccountUserNotFoundException(userAccountId);

        if (userAccount.Status == UserAccountStatus.Blocked)
            throw new UserAccountUserBlockedException(userAccountId);

        await userAccountRepository.DeleteAsync(userAccountId);

        return userAccountId;
    }

    public async Task<UserAccountDto> GetUserAccount(Guid userAccountId)
    {
        var userAccount = await userAccountRepository.GetByIdAsync(userAccountId);
        if(userAccount == null)
            throw new UserAccountUserNotFoundException(userAccountId);
        if (userAccount.Status == UserAccountStatus.Blocked)
            throw new UserAccountUserBlockedException(userAccountId);
        return MapUserAccountDto(userAccount);
    }

    private UserAccountDto MapUserAccountDto(UserAccount userAccount)
    {
        return new UserAccountDto
        {
            Email = userAccount.Email,
            Surname = userAccount.Surname,
            Name = userAccount.Name,
            SecondName = userAccount.SecondName,
            PhoneNumber = userAccount.PhoneNumber,
            BirthDate = userAccount.BirthDate,
            Gender = userAccount.Gender
        };
    }

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
            StatusChangedDate = DateTime.Now
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

    private void ValidateAge(DateTime date)
    {
        if(DateTime.Now.Year - date.Year < 18)
            throw new UserAccountInvalidAgeException(date);
        if (DateTime.Now.Year - date.Year > 120)
            throw new UserAccountInvalidAgeException(date);
        if (DateTime.Now.Year - date.Year > 18)
        {
            return;
        }

        if (DateTime.Now.Month > date.Month)
        {
            return;
        }
        if (DateTime.Now.Month < date.Month)
        {
            throw new UserAccountInvalidAgeException(date);
        }
        if (DateTime.Now.Day > date.Day)
        {
            return;
        }
        if (DateTime.Now.Day <= date.Day)
        {
            throw new UserAccountInvalidAgeException(date);
        }
        
    }
}