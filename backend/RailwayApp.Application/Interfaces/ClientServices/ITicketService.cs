using RailwayApp.Domain.Entities;

namespace RailwayApp.Domain.Interfaces.IServices;

public interface ITicketService
{
    public Task<IEnumerable<Ticket>> GetActiveTickets(Guid userAccountId);
    public Task<IEnumerable<Ticket>> GetCancelledTickets(Guid userAccountId);
    public Task<IEnumerable<Ticket>> GetExpiredTickets(Guid userAccountId);
}