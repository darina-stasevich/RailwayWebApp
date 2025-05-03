using RailwayApp.Domain.Entities;

namespace RailwayApp.Domain.Interfaces.IRepositories;

public interface IConcreteRouteSegmentRepository
{
    Task DeleteAllAsync();
    Task<Guid> CreateAsync(ConcreteRouteSegment segment);
    Task<ConcreteRouteSegment?> GetConcreteSegmentByAbstractSegmentIdAsync(Guid abstractRouteSegmentId, DateTime departureDate);
    Task<List<ConcreteRouteSegment>> GetConcreteSegmentsByAbstractSegmentIdAsync(Guid abstractSegmentId);
    Task<List<ConcreteRouteSegment>> GetConcreteSegmentsByConcreteRouteIdAsync(Guid concreteRouteId);
}