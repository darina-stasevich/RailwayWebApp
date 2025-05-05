using System.Security.Principal;
using RailwayApp.Application.Models;
using RailwayApp.Application.Models.Dto;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces.IRepositories;
using RailwayApp.Domain.Interfaces.IServices;

namespace RailwayApp.Application.Services;

public class CarriageService(ICarriageSeatService carriageSeatService,
    IPriceCalculationService priceCalculationService,
    ICarriageTemplateService carriageTemplateService
    ) : ICarriageService
{ 
    private InfoRouteSegmentSearchPerCarriageDto MapInfoRouteSegmentPerCarriageSearch(CarriageInfoRequest request, Guid carriageTemplateId)
    {
        return new InfoRouteSegmentSearchPerCarriageDto
        {
            ConcreteRouteId = request.ConcreteRouteId,
            EndSegmentNumber = request.EndSegmentNumber,
            StartSegmentNumber = request.StartSegmentNumber,
            CarriageTemplateId = carriageTemplateId
        };
    }

    private InfoRouteSegmentSearchDto MapInfoRouteSegmentSearch(CarriagesInfoRequest request)
    {
        return new InfoRouteSegmentSearchDto
        {
            ConcreteRouteId = request.ConcreteRouteId,
            StartSegmentNumber = request.StartSegmentNumber,
            EndSegmentNumber = request.EndSegmentNumber
        };
    }
    public async Task<List<ShortCarriageInfoDto>> GetAllCarriagesInfo(CarriagesInfoRequest request)
    {
        if(request.StartSegmentNumber > request.EndSegmentNumber)
            throw new ArgumentException("Start segment number must be less than end segment number");
        var carriageTemplates = await carriageTemplateService.GetCarriageTemplateForRouteAsync(request.ConcreteRouteId);

        var searchInfoDto = MapInfoRouteSegmentSearch(request);
        // key: CarriageTemplateId, value is the number of available seats in carriage with given template
        var carriageAvailableSeats =
            await carriageSeatService.GetAvailableSeatCountsPerCarriageAsync(searchInfoDto);
        
        var carriagesInfo = new List<ShortCarriageInfoDto>();

        var carriagePrices =
            await priceCalculationService.GetPricesForAllCarriageTypesAsync(searchInfoDto);
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

    public async Task<DetailedCarriageInfoDto> GetCarriageInfo(CarriageInfoRequest request)
    {
        if(request.StartSegmentNumber > request.EndSegmentNumber)
            throw new ArgumentException("Start segment number must be less than end segment number");
        
        // 1. Get this carriageTemplate
        var carriageTemplates = await carriageTemplateService.GetCarriageTemplateForRouteAsync(request.ConcreteRouteId);
        var carriageTemplate = carriageTemplates
            .FirstOrDefault(x => x.CarriageNumber == request.CarriageNumber);

        if (carriageTemplate == null)
        {
            throw new ArgumentException($"Carriage template with number {request.CarriageNumber} not found");
        }


        var searchPerCarriageDto = MapInfoRouteSegmentPerCarriageSearch(request, carriageTemplate.Id);
        // 2. Get available seats for this carriage
        var availableSeats = await carriageSeatService.GetAvailableSeatsForCarriageAsync(searchPerCarriageDto);
        
        var price = await priceCalculationService.CalculatePriceForCarriageAsync(searchPerCarriageDto);
        
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