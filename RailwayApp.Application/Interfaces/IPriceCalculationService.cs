using RailwayApp.Application.Models;

namespace RailwayApp.Domain.Interfaces.IServices;

public interface IPriceCalculationService
{
    Task<decimal> CalculatePriceForCarriageAsync(Guid concreteRouteId, int startSegmentNumber, int endSegmentNumber, Guid carriageTemplateId);

    Task<PriceRangeDto?> GetRoutePriceRangeAsync(Guid concreteRouteId, int startSegmentNumber, int endSegmentNumber);
    
    Task<Dictionary<Guid, decimal>> GetPricesForAllCarriageTypesAsync(Guid concreteRouteId, int startSegmentNumber, int endSegmentNumber);
}