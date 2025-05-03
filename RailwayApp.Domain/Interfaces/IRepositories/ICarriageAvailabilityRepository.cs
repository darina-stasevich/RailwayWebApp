using RailwayApp.Domain.Entities;

namespace RailwayApp.Domain.Interfaces.IRepositories;

public interface ICarriageAvailabilityRepository
{
    Task DeleteAllAsync();
    Task<Guid> CreateAsync(CarriageAvailability carriageAvailability);
    Task<CarriageAvailability?> GetByIdAsync(Guid id);
    Task<List<CarriageAvailability>> GetByConcreteSegmentIdAsync(Guid concreteSegmentId);
    
}