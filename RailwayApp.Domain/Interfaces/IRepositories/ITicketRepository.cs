using RailwayApp.Domain.Entities;

namespace RailwayApp.Domain.Interfaces.IRepositories;

public interface ITicketRepository
{
    Task DeleteAllAsync();
    Task<Guid> CreateAsync(Ticket ticket);
    Task<IEnumerable<Ticket>> GetByUserAccountIdAsync(Guid id);
    Task<Ticket> GetByIdAsync(Guid id);
}