using RailwayApp.Domain.Entities;

namespace RailwayApp.Domain.Interfaces.IRepositories;

public interface ITicketRepository
{
    Task CreateAsync(Ticket ticket);
    Task<IEnumerable<Ticket>> GetByUserEmailAsync(string email);
    
    Task<Ticket> GetByIdAsync(Guid id);
}