using MongoDB.Driver;
using RailwayApp.Domain.Entities;

namespace RailwayApp.Domain.Interfaces.IRepositories;

public interface IConcreteRouteSegmentRepository : IGenericRepository<ConcreteRouteSegment, Guid>
{
    Task<ConcreteRouteSegment?> GetConcreteSegmentByAbstractSegmentIdAsync(Guid abstractRouteSegmentId,
        DateTime departureDate);

    Task<IEnumerable<ConcreteRouteSegment>> GetConcreteSegmentsByAbstractSegmentIdAsync(Guid abstractSegmentId);

    Task<IEnumerable<ConcreteRouteSegment>> GetConcreteSegmentsByConcreteRouteIdAsync(Guid concreteRouteId,
        IClientSessionHandle? session = null);

    Task<IEnumerable<ConcreteRouteSegment>> GetConcreteSegmentsByFromStationAsync(Guid fromStationId);
    Task<IEnumerable<ConcreteRouteSegment>> GetConcreteSegmentsByToStationAsync(Guid toStationId);
}