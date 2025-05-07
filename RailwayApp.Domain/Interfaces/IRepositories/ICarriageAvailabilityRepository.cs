using RailwayApp.Domain.Entities;

namespace RailwayApp.Domain.Interfaces.IRepositories;

public interface ICarriageAvailabilityRepository : IGlobalRepository<CarriageAvailability, Guid>
{
    Task<List<CarriageAvailability>> GetByConcreteSegmentIdAsync(Guid concreteSegmentId);
    
}