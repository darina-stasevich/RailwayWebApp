using MongoDB.Driver;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Statuses;

namespace RailwayApp.Domain.Interfaces.IRepositories;

public interface ITicketRepository : IGenericRepository<Ticket, Guid>
{
    Task AddRange(IEnumerable<Ticket> tickets, IClientSessionHandle session);
    Task<IEnumerable<Ticket>> GetByUserAccountIdAsync(Guid id);
    Task<bool> UpdateStatusAsync(Guid id, TicketStatus status, IClientSessionHandle session);
    
}