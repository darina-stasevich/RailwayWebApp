using RailwayApp.Domain.Entities;

namespace RailwayApp.Domain.Interfaces.IRepositories;

public interface IAbstractRouteSegmentRepository : IGlobalRepository<AbstractRouteSegment, Guid>
{
    Task<List<AbstractRouteSegment>> GetAbstractSegmentsByFromStationAsync(Guid fromStationId);
    Task<List<AbstractRouteSegment>> GetAbstractSegmentsByToStationAsync(Guid toStationId);
    Task<List<AbstractRouteSegment>> GetAbstractSegmentsByRouteIdAsync(Guid routeId);
}