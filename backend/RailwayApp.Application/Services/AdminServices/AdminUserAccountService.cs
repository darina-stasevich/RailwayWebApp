using RailwayApp.Domain;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces.IRepositories;
using RailwayApp.Domain.Interfaces.IServices.AdminServices;

namespace RailwayApp.Application.Services.AdminServices;

public class AdminUserAccountService(IUserAccountRepository userAccountRepository) : IAdminUserAccountService
{
    public async Task<IEnumerable<UserAccount>> GetAllItems()
    {
        return await userAccountRepository.GetAllAsync();
    }

    public async Task<UserAccount> GetItemByIdAsync(Guid id)
    {
        var userAccount = await userAccountRepository.GetByIdAsync(id);
        if (userAccount == null)
            throw new AdminResourceNotFoundException(nameof(UserAccount), id);
        return userAccount;
    }

    private void ValidateAge(DateOnly date)
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
    public async Task<Guid> CreateItem(UserAccount item)
    {
        var existingUserAccount = await userAccountRepository.GetByIdAsync(item.Id);
        if (existingUserAccount != null)
            throw new AdminDataConflictException($"User account with ID '{item.Id}' already exists.");
        ValidateAge(item.BirthDate);
        return await userAccountRepository.AddAsync(item);
    }

    public async Task<bool> UpdateItem(Guid id, UserAccount itemToUpdate)
    {
        if (id != itemToUpdate.Id)
        {
            itemToUpdate.Id = id;
        }

        var existingUserAccount = await userAccountRepository.GetByIdAsync(id);
        if (existingUserAccount == null)
        {
            throw new AdminResourceNotFoundException(nameof(UserAccount), id);
        }

        ValidateAge(itemToUpdate.BirthDate);
        itemToUpdate.Email = existingUserAccount.Email;
        itemToUpdate.HashedPassword = existingUserAccount.HashedPassword;
       
        bool success = await userAccountRepository.UpdateAsync(itemToUpdate);
        return success;
    }

    public async Task<Guid> DeleteItem(Guid id)
    {
        var existingSegment = await userAccountRepository.GetByIdAsync(id);
        if (existingSegment == null)
        {
            throw new AdminResourceNotFoundException(nameof(UserAccount), id);
        }
        
        await userAccountRepository.DeleteAsync(id);
        return id;
    }
}