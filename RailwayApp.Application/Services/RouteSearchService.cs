using System.Collections;
using System.Data;
using MongoDB.Driver.Linq;
using RailwayApp.Application.Models;
using RailwayApp.Application.Models.Dto;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces.IRepositories;
using RailwayApp.Domain.Interfaces.IServices;

namespace RailwayApp.Application.Services;

public class RouteSearchService(IStationRepository stationRepository,
    IAbstractRouteSegmentRepository abstractRouteSegmentRepository,
    IConcreteRouteSegmentRepository concreteRouteSegmentRepository,
    IPriceCalculationService priceCalculationService,
    ICarriageSeatService carriageSeatService) : IRouteSearchService
{
    private InfoRouteSegmentSearchDto MapInfoRouteSegmentSearch(Guid routeId, int startSegmentNumber, int endSegmentNumber)
    {
        return new InfoRouteSegmentSearchDto
        {
            ConcreteRouteId = routeId,
            EndSegmentNumber = endSegmentNumber,
            StartSegmentNumber = startSegmentNumber
        };
    }
    
    private InfoRouteSegmentSearchPerCarriageDto MapInfoRouteSegmentPerCarriageSearch(Guid routeId, int startSegmentNumber, int endSegmentNumber, Guid carriageTemplateId)
    {
        return new InfoRouteSegmentSearchPerCarriageDto
        {
            ConcreteRouteId = routeId,
            EndSegmentNumber = endSegmentNumber,
            StartSegmentNumber = startSegmentNumber,
            CarriageTemplateId = carriageTemplateId
        };
    }
    
    public Task<List<ComplexRouteDto>> GetRoutesAsync(RouteSearchRequest request)
    {
        var fromStation = stationRepository.GetByIdAsync(request.FromStationId).Result;
        if (fromStation == null)
        {
            throw new ArgumentException("From station not found");
        }
        var toStation = stationRepository.GetByIdAsync(request.ToStationId).Result;
        if (toStation == null)
        {
            throw new ArgumentException("To station not found");
        }
        if(request.DepartureDate - DateTime.Today > TimeSpan.FromDays(30))
        {
            throw new ArgumentException("Departure date cannot be more than 30 days in the future");
        }
        if(request.DepartureDate - DateTime.Today < TimeSpan.FromDays(0))
        {
            throw new ArgumentException("Departure date cannot be in the past");
        }

        if (request.IsDirectRoute)
        {
            return GetDirectRoutes(fromStation, toStation, request.DepartureDate);
        }
        else
        {
            return GetComplexRoutes(fromStation, toStation, request.DepartureDate);
        }
    }
    
    private async Task<List<ComplexRouteDto>> GetDirectRoutes(Station fromStation, Station toStation, DateTime departureDate)
    {
        // 1. Find abstract route segments that start in fromStation
        var startAbstractRouteSegments =
            await abstractRouteSegmentRepository.GetAbstractSegmentsByFromStationAsync(fromStation.Id);
        
        // 2. Find concrete route segments that start in departure date
        var startConcreteRouteSegments = new List<ConcreteRouteSegment>();
        foreach (var abstractSegment in startAbstractRouteSegments)
        {
            var concreteSegment = await concreteRouteSegmentRepository.GetConcreteSegmentByAbstractSegmentIdAsync(abstractSegment.Id,
                    departureDate);
            if(concreteSegment != null)
                startConcreteRouteSegments.Add(concreteSegment);
        }
        
        // 3. Get concrete route id
        var startConcreteRouteIds = new HashSet<Guid>(
            startConcreteRouteSegments.Select(s => s.ConcreteRouteId)
        );
        
        // 4. Find abstract route segments that end in toStation
        var endAbstractRouteSegments =
            await abstractRouteSegmentRepository.GetAbstractSegmentsByToStationAsync(toStation.Id);
        
        // 5. Find concrete end route segments that are in start concrete route segments
        var endConcreteRouteSegments = new List<ConcreteRouteSegment>();
        foreach (var abstractSegment in endAbstractRouteSegments)
        {
            var concreteSegments =
                await concreteRouteSegmentRepository.GetConcreteSegmentsByAbstractSegmentIdAsync(abstractSegment.Id);
            var concreteSegment = concreteSegments
                .FirstOrDefault(s => startConcreteRouteIds.Contains(s.ConcreteRouteId));
            if (concreteSegment != null)
                endConcreteRouteSegments.Add(concreteSegment);
        }
        
        // 6. get concrete routes that has both start and end segments
        var bothConcreteRouteIds = new HashSet<Guid>(
            endConcreteRouteSegments.Select(s => s.ConcreteRouteId));
        
        // 8. check that start segment has number less than end segment
        // 9. for valid routes calculate cost and search for available seats
        // 10. generate dto
        
        // using AbstractRouteSegmentId can get number of segment
        // using ConcreteRouteId can get start and end segments' ids of route
        // using ConcreteRouteSegmentId can get ConcreteRouteSegment
        var startAbstractRouteSegmentsNumbers = startAbstractRouteSegments.ToDictionary(k => k.Id, v => v.SegmentNumber);
        var endAbstractRouteSegmentsNumbers = endAbstractRouteSegments.ToDictionary(k => k.Id, v => v.SegmentNumber);
        var startRouteDict = startConcreteRouteSegments.ToDictionary(k => k.ConcreteRouteId,
            v => v.Id);
        var endRouteDict = endConcreteRouteSegments.ToDictionary(k => k.ConcreteRouteId,
            v => v.Id);
        var startConcreteSegmentsDict = startConcreteRouteSegments.ToDictionary(s => s.Id);
        var endConcreteSegmentsDict = endConcreteRouteSegments.ToDictionary(s => s.Id);
        var directRoutes = new List<ComplexRouteDto>();
        foreach (var routeId in bothConcreteRouteIds)
        {
            var startNumber =
                startAbstractRouteSegmentsNumbers[startConcreteSegmentsDict[startRouteDict[routeId]].AbstractSegmentId];
            var endNumber =
                endAbstractRouteSegmentsNumbers[endConcreteSegmentsDict[endRouteDict[routeId]].AbstractSegmentId];
            var startSegment = startConcreteSegmentsDict[startRouteDict[routeId]];
            var endSegment = endConcreteSegmentsDict[endRouteDict[routeId]];
            if(startNumber <= endNumber)
            {
                var infoRouteSegmentDto = MapInfoRouteSegmentSearch(routeId, startNumber, endNumber);
                var costRange = await priceCalculationService.GetRoutePriceRangeAsync(infoRouteSegmentDto);
                int availableSeats = await carriageSeatService.GetAvailableSeatsAmountAsync(infoRouteSegmentDto);
                if (costRange == null)
                {
                    throw new Exception("Price range not found");
                }

                var routeDto = new DirectRouteDto
                {
                    ConcreteRouteId = routeId,
                    StartSegmentNumber = startNumber,
                    EndSegmentNumber = endNumber,
                    DepartureDate = startSegment.ConcreteDepartureDate,
                    ArrivalDate = endSegment.ConcreteArrivalDate,
                    TimeInTransit = endSegment.ConcreteArrivalDate - startSegment.ConcreteDepartureDate,
                    MinimalCost = costRange.MinimalPrice,
                    MaximumCost = costRange.MaximumPrice,
                    AvailableSeats = availableSeats
                };

                var complexRoute = CreateComplexRouteDto(new List<DirectRouteDto>{routeDto});
                directRoutes.Add(complexRoute);
            }
        }
        return directRoutes;
    }

    private ComplexRouteDto CreateComplexRouteDto(IEnumerable<DirectRouteDto> directRoutes)
    {
        var complexRoute = new ComplexRouteDto
        {
            DirectRoutes = directRoutes.ToList(),
            MinimalTotalCost = directRoutes.Sum(r => r.MinimalCost),
            MaximumTotalCost = directRoutes.Sum(r => r.MaximumCost),
            DepartureDate = directRoutes.Min(r => r.DepartureDate),
            ArrivalDate = directRoutes.Max(r => r.ArrivalDate),
            TotalDuration = directRoutes.Max(r => r.ArrivalDate) - directRoutes.Min(r => r.DepartureDate)
        };
        return complexRoute;
    }
    
    private Task<List<ComplexRouteDto>> GetComplexRoutes(Station fromStation, Station toStation, DateTime departureDate)
    {
        throw new NotImplementedException("Complex route search is not implemented yet");
    }
}