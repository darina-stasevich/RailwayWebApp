using MongoDB.Driver.Linq;
using RailwayApp.Domain;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces.IRepositories;
using RailwayApp.Domain.Interfaces.IServices;
using RailwayApp.Domain.Statuses;

namespace RailwayApp.Application.Services;

public class TicketService(ITicketRepository ticketRepository,
    IUserAccountRepository userAccountRepository) : ITicketService
{
    private async Task<IEnumerable<Ticket>> GetAllTickets(Guid userAccountId)
    {
        return await ticketRepository.GetByUserAccountIdAsync(userAccountId);
    }

    private async Task VerifyUserAccount(Guid userAccountId)
    {
        var userAccount = await userAccountRepository.GetByIdAsync(userAccountId);
        if (userAccount == null)
        {
            throw new UserAccountUserNotFoundException(userAccountId);
        }
        if (userAccount.Status == UserAccountStatus.Blocked)
        {
            throw new UserAccountUserBlockedException(userAccountId);
        }
    }
    public async Task<IEnumerable<Ticket>> GetActiveTickets(Guid userAccountId)
    {
        await VerifyUserAccount(userAccountId);
        var tickets = await GetAllTickets(userAccountId);
        return tickets.Where(x => x.Status == TicketStatus.Payed).ToList();
    }

    public async Task<IEnumerable<Ticket>> GetCancelledTickets(Guid userAccountId)
    {
        await VerifyUserAccount(userAccountId);
        var tickets = await GetAllTickets(userAccountId);
        return tickets.Where(x => x.Status == TicketStatus.Cancelled).ToList();
    }

    public async Task<IEnumerable<Ticket>> GetExpiredTickets(Guid userAccountId)
    {
        await VerifyUserAccount(userAccountId);
        var tickets = await GetAllTickets(userAccountId);
        return tickets.Where(x => x.Status == TicketStatus.Expired).ToList();

    }
}