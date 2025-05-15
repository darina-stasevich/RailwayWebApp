using MongoDB.Driver;
using RailwayApp.Domain.Entities;

namespace RailwayApp.Domain.Interfaces.IRepositories;

public interface IAbstractRouteSegmentRepository : IGenericRepository<AbstractRouteSegment, Guid>
{
    Task<IEnumerable<AbstractRouteSegment>> GetAbstractSegmentsByFromStationAsync(Guid fromStationId);
    Task<IEnumerable<AbstractRouteSegment>> GetAbstractSegmentsByToStationAsync(Guid toStationId);

    Task<IEnumerable<AbstractRouteSegment>> GetAbstractSegmentsByRouteIdAsync(Guid routeId,
        IClientSessionHandle? session = null);
}