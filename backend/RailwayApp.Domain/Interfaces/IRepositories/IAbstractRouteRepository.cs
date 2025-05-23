using RailwayApp.Domain.Entities;

namespace RailwayApp.Domain.Interfaces.IRepositories;

public interface IAbstractRouteRepository : IGenericRepository<AbstractRoute, Guid>
{
    public Task<IEnumerable<AbstractRoute>> GetActiveRoutes();
}