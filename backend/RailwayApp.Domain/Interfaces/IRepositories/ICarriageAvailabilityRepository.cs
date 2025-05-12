using MongoDB.Driver;
using RailwayApp.Domain.Entities;

namespace RailwayApp.Domain.Interfaces.IRepositories;

public interface ICarriageAvailabilityRepository : IGenericRepository<CarriageAvailability, Guid>
{
    Task<IEnumerable<CarriageAvailability>> GetByConcreteSegmentIdAsync(Guid concreteSegmentId, IClientSessionHandle? session = null);

    Task<CarriageAvailability> GetByConcreteSegmentIdAndTemplateIdAsync(Guid segmentId, Guid carriageTemplateId,
        IClientSessionHandle session);

    Task<bool> UpdateOccupiedSeats(IEnumerable<CarriageAvailability> carriageAvailabilities, IClientSessionHandle session);
}