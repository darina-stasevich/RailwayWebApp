using RailwayApp.Domain.Entities;

namespace RailwayApp.Domain.Interfaces.IRepositories;

public interface IAbstractRouteRepository
{
    Task DeleteAllAsync();
    Task<Guid> CreateAsync(AbstractRoute route);
    Task<AbstractRoute?> GetByIdAsync(Guid id);
}