using MongoDB.Driver;
using RailwayApp.Application.Models;
using RailwayApp.Application.Models.Dto;

namespace RailwayApp.Domain.Interfaces.IServices;

public interface IPriceCalculationService
{
    Task<decimal> CalculatePriceForCarriageAsync(InfoRouteSegmentSearchPerCarriageDto dto, IClientSessionHandle? session = null);

    Task<PriceRangeDto?> GetRoutePriceRangeAsync(InfoRouteSegmentSearchDto dto);
    
    Task<Dictionary<Guid, decimal>> GetPricesForAllCarriageTypesAsync(InfoRouteSegmentSearchDto dto);
}