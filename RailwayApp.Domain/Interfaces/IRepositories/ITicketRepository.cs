using RailwayApp.Domain.Entities;

namespace RailwayApp.Domain.Interfaces.IRepositories;

public interface ITicketRepository : IGenericRepository<Ticket, Guid>
{
    Task<IEnumerable<Ticket>> GetByUserAccountIdAsync(Guid id);
}