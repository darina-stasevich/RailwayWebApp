using RailwayApp.Application.Models;
using RailwayApp.Domain.Entities;

namespace RailwayApp.Domain.Interfaces.IServices;

public interface ITicketService
{
    public Task<IEnumerable<TicketDto>> GetActiveTickets(Guid userAccountId);
    public Task<IEnumerable<TicketDto>> GetCancelledTickets(Guid userAccountId);
    public Task<IEnumerable<TicketDto>> GetExpiredTickets(Guid userAccountId);
}