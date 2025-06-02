using MongoDB.Driver;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Statuses;

namespace RailwayApp.Domain.Interfaces.IRepositories;

public interface ITicketRepository : IGenericRepository<Ticket, Guid>
{
    Task AddRange(IEnumerable<Ticket> tickets, IClientSessionHandle session);
    Task<IEnumerable<Ticket>> GetByUserAccountIdAsync(Guid userAccountId);
    Task<bool> UpdateStatusAsync(Guid id, TicketStatus status, IClientSessionHandle session);
    Task<IEnumerable<Ticket>> GetPayedTicketsPastDepartureAsync(DateTime currentUtcTime);
    Task<long> UpdateStatusesToExpiredAsync(IEnumerable<Guid> ticketIds, IClientSessionHandle session = null);
}