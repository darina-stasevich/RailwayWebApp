using RailwayApp.Domain.Entities;

namespace RailwayApp.Domain.Interfaces.IRepositories;

public interface ICarriageAvailabilityRepository : IGenericRepository<CarriageAvailability, Guid>
{
    Task<IEnumerable<CarriageAvailability>> GetByConcreteSegmentIdAsync(Guid concreteSegmentId);
    
}