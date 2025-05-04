using RailwayApp.Domain.Entities;

namespace RailwayApp.Domain.Interfaces.IServices;

public interface ICarriageSeatService
{
     Task<int> GetAvailableSeatsAmountAsync(Guid concreteRouteId, int startSegmentNumber, int endSegmentNumber);
     Task<Dictionary<Guid, int>> GetAvailableSeatCountsPerCarriageAsync(Guid concreteRouteId, int startSegmentNumber,
          int endSegmentNumber);
     Task<List<int>> GetAvailableSeatsForCarriageAsync(Guid concreteRouteId, int startSegmentNumber, int endSegmentNumber,
          Guid carriageTemplateId);
     Task<bool> IsSeatAvailable(Guid concreteRouteId, int startSegmentNumber, int endSegmentNumber, int carriageNumber, int seatNumber);
}