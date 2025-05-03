using System.Collections;
using MongoDB.Driver.Linq;
using RailwayApp.Application.Models;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces.IRepositories;
using RailwayApp.Domain.Interfaces.IServices;

namespace RailwayApp.Application.Services;

public class RouteSearchService(IStationRepository stationRepository,
    IAbstractRouteSegmentRepository abstractRouteSegmentRepository,
    IConcreteRouteSegmentRepository concreteRouteSegmentRepository,
    IAbstractRouteRepository abstractRouteRepository,
    IConcreteRouteRepository concreteRouteRepository,
    ICarriageAvailabilityRepository carriageAvailabilityRepository) : IRouteSearchService
{
    public Task<List<ComplexRouteDto>> GetRoutesAsync(Guid fromStationId, Guid toStationId, DateTime departureDate, bool isDirectRoute)
    {
        var fromStation = stationRepository.GetByIdAsync(fromStationId).Result;
        if (fromStation == null)
        {
            throw new ArgumentException("From station not found");
        }
        var toStation = stationRepository.GetByIdAsync(toStationId).Result;
        if (toStation == null)
        {
            throw new ArgumentException("To station not found");
        }
        if(departureDate - DateTime.Today > TimeSpan.FromDays(30))
        {
            throw new ArgumentException("Departure date cannot be more than 30 days in the future");
        }
        if(departureDate - DateTime.Today < TimeSpan.FromDays(0))
        {
            throw new ArgumentException("Departure date cannot be in the past");
        }

        if (isDirectRoute)
        {
            return GetDirectRoutes(fromStation, toStation, departureDate);
        }
        else
        {
            return GetComplexRoutes(fromStation, toStation, departureDate);
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
                decimal cost = await CalculateCost(routeId, startNumber, endNumber);
                int availableSeats = await CalculateSeats(routeId, startNumber, endNumber);
                var routeDto = new DirectRouteDto
                {
                    ConcreteRouteId = routeId,
                    FromStationId = fromStation.Id,
                    ToStationId = toStation.Id,
                    DepartureDate = startSegment.ConcreteDepartureDate,
                    ArrivalDate = endSegment.ConcreteArrivalDate,
                    TimeInTransit = endSegment.ConcreteArrivalDate - startSegment.ConcreteDepartureDate,
                    Cost = cost,
                    AvailableSeats = availableSeats
                };

                var complexRoute = CreateComplexRouteDto(new List<DirectRouteDto>{routeDto});
                directRoutes.Add(complexRoute);
            }
        }
        
        // 11. return dto
        return directRoutes;
    }

    private ComplexRouteDto CreateComplexRouteDto(IEnumerable<DirectRouteDto> directRoutes)
    {
        var complexRoute = new ComplexRouteDto
        {
            DirectRoutes = directRoutes.ToList(),
            TotalCost = directRoutes.Sum(r => r.Cost),
            DepartureDate = directRoutes.Min(r => r.DepartureDate),
            ArrivalDate = directRoutes.Max(r => r.ArrivalDate),
            TotalDuration = directRoutes.Max(r => r.ArrivalDate) - directRoutes.Min(r => r.DepartureDate)
        };
        return complexRoute;
    }
    private async Task<int> CalculateSeats(Guid concreteRouteId, int startSegmentNumber, int endSegmentNumber)
    {
        // 1. Get concrete route    
        var concreteRoute = await concreteRouteRepository.GetByIdAsync(concreteRouteId);
        if (concreteRoute == null)
        {
            throw new ArgumentException("Concrete route not found");
        }

        // 2. Get relevant abstract route
        var abstractRoute = await abstractRouteRepository.GetByIdAsync(concreteRoute.AbstractRouteId);
        if (abstractRoute == null)
        {
            throw new ArgumentException("Abstract route not found");
        }
        
        // 3. Get relevant abstract route segments (has segment numbers)
        Console.WriteLine(startSegmentNumber);
        Console.WriteLine(endSegmentNumber);
        var abstractRouteSegments =
            await abstractRouteSegmentRepository.GetAbstractSegmentsByRouteIdAsync(abstractRoute.Id);
        foreach (var element in abstractRouteSegments)
        {
            Console.WriteLine(element.SegmentNumber);
        }
        var relevantAbstractRouteSegments = abstractRouteSegments
            .Where(s => s.SegmentNumber >= startSegmentNumber && s.SegmentNumber <= endSegmentNumber);
        var segmentsHashSet = new HashSet<Guid>(relevantAbstractRouteSegments.Select(s => s.Id));
        
        // 4. Get relevant concrete route segments
        var relevantConcreteRouteSegments =
            (await concreteRouteSegmentRepository.GetConcreteSegmentsByConcreteRouteIdAsync(concreteRouteId))
            .Where(s => segmentsHashSet.Contains(s.AbstractSegmentId));

        // 5. get all availabilities for segments
        var allAvailabilities = new List<CarriageAvailability>();
        foreach (var routeSegment in relevantConcreteRouteSegments)
        {
            var carriageAvailabilities = await carriageAvailabilityRepository.GetByConcreteSegmentIdAsync(routeSegment.Id);
            allAvailabilities.AddRange(carriageAvailabilities);
        }
        
        // 6. Calculate seats that are free every segment in range
        var availabilityGroupedByCarriage = allAvailabilities
            .GroupBy(a => a.CarriageTemplateId);

        int totalAvailableSeats = 0;

        foreach (var group in availabilityGroupedByCarriage)
        {
            var groupList = group.ToList();
            var mask = new BitArray(groupList[0].OccupiedSeats);
            for (int i = 1; i < groupList.Count; i++)
            {
                mask = mask.And(groupList[i].OccupiedSeats);
            }

            totalAvailableSeats += CountSetBits(mask);
        }

        return totalAvailableSeats;
    }
    
    private int CountSetBits(BitArray bitArray)
    {
        int count = 0;
        foreach (bool bit in bitArray)
        {
            if (bit)
            {
                count++;
            }
        }
        return count;
    }
    private async Task<decimal> CalculateCost(Guid concreteRouteId, int startSegmentNumber, int endSegmentNumber)
    {
        var concreteRoute = await concreteRouteRepository.GetByIdAsync(concreteRouteId);
        if (concreteRoute == null)
        {
            throw new ArgumentException("Concrete route not found");
        }
        var abstractRoute = await abstractRouteRepository.GetByIdAsync(concreteRoute.AbstractRouteId);
        if (abstractRoute == null)
        {
            throw new ArgumentException("Abstract route not found");
        }

        decimal transferCost = abstractRoute.TransferCost;
        var abstractRouteSegments =
            await abstractRouteSegmentRepository.GetAbstractSegmentsByRouteIdAsync(abstractRoute.Id);
        var price = abstractRouteSegments
            .Where(s => s.SegmentNumber >= startSegmentNumber && s.SegmentNumber <= endSegmentNumber)
            .Sum(s => s.SegmentCost);
        return price + transferCost;
    }
    private Task<List<ComplexRouteDto>> GetComplexRoutes(Station fromStation, Station toStation, DateTime departureDate)
    {
        throw new NotImplementedException("Complex route search is not implemented yet");
    }
}