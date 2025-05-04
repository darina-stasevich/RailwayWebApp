using System.Security.Principal;
using RailwayApp.Application.Models;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces.IRepositories;
using RailwayApp.Domain.Interfaces.IServices;

namespace RailwayApp.Application.Services;

public class CarriageService(ICarriageSeatService carriageSeatService,
    IPriceCalculationService priceCalculationService,
    ICarriageTemplateService carriageTemplateService
    ) : ICarriageService
{ 
    public async Task<List<ShortCarriageInfoDto>> GetAllCarriagesInfo(Guid concreteRouteId, int startSegmentNumber, int endSegmentNumber)
    {
        if(startSegmentNumber > endSegmentNumber)
            throw new ArgumentException("Start segment number must be less than end segment number");
        var carriageTemplates = await carriageTemplateService.GetCarriageTemplateForRouteAsync(concreteRouteId);
        
        // key: CarriageTemplateId, value is the number of available seats in carriage with given template
        var carriageAvailableSeats =
            await carriageSeatService.GetAvailableSeatCountsPerCarriageAsync(concreteRouteId, startSegmentNumber,
                endSegmentNumber);
        
        var carriagesInfo = new List<ShortCarriageInfoDto>();
        var carriagePrices =
            await priceCalculationService.GetPricesForAllCarriageTypesAsync(concreteRouteId, startSegmentNumber,
                endSegmentNumber);
        foreach (var carriageTemplate in carriageTemplates)
        {
            var dto = new ShortCarriageInfoDto
            {
                LayoutIdentifier = carriageTemplate.LayoutIdentifier,
                CarriageNumber = carriageTemplate.CarriageNumber,
                AvailableSeats = carriageAvailableSeats[carriageTemplate.Id],
                Cost = carriagePrices[carriageTemplate.Id]
            };
            carriagesInfo.Add(dto);
        }

        return carriagesInfo.OrderBy(d => d.CarriageNumber).ToList();
    }

    public async Task<DetailedCarriageInfoDto> GetCarriageInfo(Guid concreteRouteId, int startSegmentNumber, int endSegmentNumber, int carriageNumber)
    {
        if(startSegmentNumber > endSegmentNumber)
            throw new ArgumentException("Start segment number must be less than end segment number");
        
        // 1. Get this carriageTemplate
        var carriageTemplates = await carriageTemplateService.GetCarriageTemplateForRouteAsync(concreteRouteId);
        var carriageTemplate = carriageTemplates
            .FirstOrDefault(x => x.CarriageNumber == carriageNumber);

        if (carriageTemplate == null)
        {
            throw new ArgumentException($"Carriage template with number {carriageNumber} not found");
        }

        // 2. Get available seats for this carriage
        var availableSeats = await carriageSeatService.GetAvailableSeatsForCarriageAsync(concreteRouteId, startSegmentNumber,
            endSegmentNumber, carriageTemplate.Id);

        var price = await priceCalculationService.CalculatePriceForCarriageAsync(concreteRouteId, startSegmentNumber,
            endSegmentNumber, carriageTemplate.Id);
        
        var detailedCarriageDto = new DetailedCarriageInfoDto
        {
            CarriageNumber = carriageTemplate.CarriageNumber,
            LayoutIdentifier = carriageTemplate.LayoutIdentifier,
            TotalSeats = carriageTemplate.TotalSeats,
            AvailableSeats = availableSeats,
            Cost = price
        };

        return detailedCarriageDto;
    }
}