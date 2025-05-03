using RailwayApp.Domain.Entities;

namespace RailwayApp.Domain.Interfaces.IRepositories;

public interface IConcreteRouteRepository
{    
    Task DeleteAllAsync();
    Task<Guid> CreateAsync(ConcreteRoute route);
    Task<ConcreteRoute?> GetByIdAsync(Guid id);
}