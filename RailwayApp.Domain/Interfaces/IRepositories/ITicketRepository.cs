using MongoDB.Driver;
using RailwayApp.Domain.Entities;

namespace RailwayApp.Domain.Interfaces.IRepositories;

public interface ITicketRepository : IGenericRepository<Ticket, Guid>
{
    Task AddRange(IEnumerable<Ticket> tickets, IClientSessionHandle session);
    Task<IEnumerable<Ticket>> GetByUserAccountIdAsync(Guid id);
}