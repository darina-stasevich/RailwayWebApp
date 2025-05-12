using RailwayApp.Domain.Entities;

namespace RailwayApp.Domain.Interfaces.IRepositories;

public interface IConcreteRouteRepository : IGenericRepository<ConcreteRoute, Guid>
{
    public Task<IEnumerable<ConcreteRoute>> GetConcreteRoutesInDate(DateTime startDate, DateTime endDate);
}