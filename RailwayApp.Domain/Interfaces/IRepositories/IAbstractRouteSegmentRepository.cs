using RailwayApp.Domain.Entities;

namespace RailwayApp.Domain.Interfaces.IRepositories;

public interface IAbstractRouteSegmentRepository
{
    Task DeleteAllAsync();
    Task<Guid> CreateAsync(AbstractRouteSegment segment);
    Task<List<AbstractRouteSegment>> GetAbstractSegmentsByFromStationAsync(Guid fromStationId);
    Task<List<AbstractRouteSegment>> GetAbstractSegmentsByToStationAsync(Guid toStationId);
    Task<List<AbstractRouteSegment>> GetAbstractSegmentsByRouteIdAsync(Guid routeId);
    Task<AbstractRouteSegment?> GetByIdAsync(Guid id);
}