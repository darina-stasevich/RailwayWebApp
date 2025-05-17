using RailwayApp.Domain;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces.IRepositories;
using RailwayApp.Domain.Interfaces.IServices.AdminServices;

namespace RailwayApp.Application.Services.AdminServices;

public class AdminUserAccountService(IUserAccountRepository userAccountRepository) : IAdminService<UserAccount, Guid>, IAdminUserAccountService
{
    public async Task<IEnumerable<UserAccount>> GetAllItems()
    {
        return await userAccountRepository.GetAllAsync();
    }

    public async Task<Guid> CreateItem(UserAccount item)
    {
        var existingUserAccount = await userAccountRepository.GetByIdAsync(item.Id);
        if (existingUserAccount != null)
            throw new AdminDataConflictException($"User account with ID '{item.Id}' already exists.");
        if (DateTime.Now - item.BirthDate < TimeSpan.FromDays(18 * 365))
            throw new AdminDataConflictException($"User must be at least 18 years old");
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