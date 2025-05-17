using RailwayApp.Domain;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces.IRepositories;
using RailwayApp.Domain.Interfaces.IServices.AdminServices;
using RailwayApp.Domain.Statuses;

namespace RailwayApp.Application.Services.AdminServices;

public class AdminTicketService(
    ITicketRepository ticketRepository,
    IConcreteRouteRepository concreteRouteRepository,
    IUserAccountRepository userAccountRepository) : IAdminService<Ticket, Guid>, IAdminTicketService
{
    public async Task<IEnumerable<Ticket>> GetAllItems()
    {
        return await ticketRepository.GetAllAsync();
    }

private async Task ValidateTicketData(Ticket item, bool isUpdate = false, Guid? existingItemId = null)
    {
        
        var userAccount = await userAccountRepository.GetByIdAsync(item.UserAccountId);
        if (userAccount == null)
            throw new AdminResourceNotFoundException(nameof(UserAccount), item.UserAccountId);
        if (userAccount.Role != UserRole.Client)
            throw new AdminValidationException(
                $"UserAccount ID '{item.UserAccountId}' does not have the required role '{nameof(UserRole.Client)}'. Actual role: '{userAccount.Role}'.");

        var concreteRoute = await concreteRouteRepository.GetByIdAsync(item.RouteId);
        if (concreteRoute == null)
            throw new AdminResourceNotFoundException(nameof(ConcreteRoute), item.RouteId);
        
    }

    public async Task<Guid> CreateItem(Ticket item)
    {
        await ValidateTicketData(item, isUpdate: false);

        return await ticketRepository.AddAsync(item);
    }

    public async Task<bool> UpdateItem(Guid id, Ticket itemToUpdate)
    {
        if (id != itemToUpdate.Id)
        {
            itemToUpdate.Id = id;
        }

        var existingTicket = await ticketRepository.GetByIdAsync(id);
        if (existingTicket == null)
        {
            throw new AdminResourceNotFoundException(nameof(Ticket), id);
        }

       
        bool success = await ticketRepository.UpdateAsync(itemToUpdate);
        return success;
    }
    
    public async Task<Guid> DeleteItem(Guid id)
    {
        var existingSegment = await ticketRepository.GetByIdAsync(id);
        if (existingSegment == null)
        {
            throw new AdminResourceNotFoundException(nameof(Ticket), id);
        }
        
        await ticketRepository.DeleteAsync(id);
        return id;
    }
}