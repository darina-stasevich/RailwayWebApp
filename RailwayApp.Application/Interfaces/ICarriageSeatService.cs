using System.Collections;
using MongoDB.Driver;
using RailwayApp.Application.Models.Dto;
using RailwayApp.Domain.Entities;

namespace RailwayApp.Domain.Interfaces.IServices;

public interface ICarriageSeatService
{
     Task<int> GetAvailableSeatsAmountAsync(InfoRouteSegmentSearchDto dto);
     Task<Dictionary<Guid, int>> GetAvailableSeatCountsPerCarriageAsync(InfoRouteSegmentSearchDto dto);
     Task<IEnumerable<int>> GetAvailableSeatsForCarriageAsync(InfoRouteSegmentSearchPerCarriageDto dto, IClientSessionHandle? session = null);
     Task<bool> IsSeatAvailable(InfoSeatSearchDto dto, IClientSessionHandle? session = null);
     
}