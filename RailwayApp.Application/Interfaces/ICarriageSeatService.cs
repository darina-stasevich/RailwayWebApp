using RailwayApp.Application.Models.Dto;
using RailwayApp.Domain.Entities;

namespace RailwayApp.Domain.Interfaces.IServices;

public interface ICarriageSeatService
{
     Task<int> GetAvailableSeatsAmountAsync(InfoRouteSegmentSearchDto dto);
     Task<Dictionary<Guid, int>> GetAvailableSeatCountsPerCarriageAsync(InfoRouteSegmentSearchDto dto);
     Task<List<int>> GetAvailableSeatsForCarriageAsync(InfoRouteSegmentSearchPerCarriageDto dto);
     Task<bool> IsSeatAvailable(Guid concreteRouteId, int startSegmentNumber, int endSegmentNumber, int carriageNumber, int seatNumber);
}