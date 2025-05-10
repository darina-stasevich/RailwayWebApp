using Microsoft.VisualBasic;
using MongoDB.Driver;
using RailwayApp.Application.Models;
using RailwayApp.Application.Models.Dto;
using RailwayApp.Domain.Interfaces.IRepositories;
using RailwayApp.Domain.Interfaces.IServices;

namespace RailwayApp.Application.Services;

public class PriceCalculationService(IConcreteRouteRepository concreteRouteRepository,
    IAbstractRouteRepository abstractRouteRepository,
    IAbstractRouteSegmentRepository abstractRouteSegmentRepository,
    ICarriageTemplateRepository carriageTemplateRepository,
    ICarriageTemplateService carriageTemplateService) : IPriceCalculationService
{
    private async Task<decimal> CalculateBasePriceAsync(Guid concreteRouteId, int startSegmentNumber, int endSegmentNumber, IClientSessionHandle? session = null)
    {
        var concreteRoute = await concreteRouteRepository.GetByIdAsync(concreteRouteId, session);
        if (concreteRoute == null)
        {
            throw new ArgumentException("Concrete route not found");
        }
        var abstractRoute = await abstractRouteRepository.GetByIdAsync(concreteRoute.AbstractRouteId, session);
        if (abstractRoute == null)
        {
            throw new ArgumentException("Abstract route not found");
        }

        decimal transferCost = abstractRoute.TransferCost;
        var abstractRouteSegments =
            await abstractRouteSegmentRepository.GetAbstractSegmentsByRouteIdAsync(abstractRoute.Id, session);
        var price = abstractRouteSegments
            .Where(s => s.SegmentNumber >= startSegmentNumber && s.SegmentNumber <= endSegmentNumber)
            .Sum(s => s.SegmentCost);
        return price + transferCost;
    }
    
    public async Task<decimal> CalculatePriceForCarriageAsync(InfoRouteSegmentSearchPerCarriageDto dto, IClientSessionHandle? session = null)
    {
        var baseCost = await CalculateBasePriceAsync(dto.ConcreteRouteId, dto.StartSegmentNumber, dto.EndSegmentNumber, session);
        var carriageTemplate = await carriageTemplateRepository.GetByIdAsync(dto.CarriageTemplateId, session);
        if(carriageTemplate == null)
            throw new Exception("Carriage template not found");
        return baseCost * carriageTemplate.PriceMultiplier;
    }

    public async Task<PriceRangeDto?> GetRoutePriceRangeAsync(InfoRouteSegmentSearchDto dto)
    {
        var baseCost = await CalculateBasePriceAsync(dto.ConcreteRouteId, dto.StartSegmentNumber, dto.EndSegmentNumber);
        var carriageTemplates = await carriageTemplateService.GetCarriageTemplateForRouteAsync(dto.ConcreteRouteId);
       
        var priceRange = new PriceRangeDto
        {
            MinimalPrice = Decimal.MaxValue,
            MaximumPrice = Decimal.MinValue
        };
        
        foreach (var carriageTemplate in carriageTemplates)
        {
            var carriagePrice = baseCost * carriageTemplate.PriceMultiplier;
            priceRange.MinimalPrice = Decimal.Min(priceRange.MinimalPrice, carriagePrice);
            priceRange.MaximumPrice = Decimal.Max(priceRange.MaximumPrice, carriagePrice);
        }

        return priceRange;
    }

    public async Task<Dictionary<Guid, decimal>> GetPricesForAllCarriageTypesAsync(InfoRouteSegmentSearchDto dto)
    {
        var baseCost = await CalculateBasePriceAsync(dto.ConcreteRouteId, dto.StartSegmentNumber, dto.EndSegmentNumber);
        var carriageTemplates = await carriageTemplateService.GetCarriageTemplateForRouteAsync(dto.ConcreteRouteId);

        var prices = new Dictionary<Guid, decimal>();
        foreach (var carriageTemplate in carriageTemplates)
        {
            prices[carriageTemplate.Id] = baseCost * carriageTemplate.PriceMultiplier;
        }

        return prices;
    }
}