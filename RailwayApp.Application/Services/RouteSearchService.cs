using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Runtime.InteropServices.JavaScript;
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
    
    public async Task<IEnumerable<ComplexRouteDto>> GetRoutesAsync(RouteSearchRequest request)
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
        if(request.DepartureDate - DateTime.UtcNow.Date > TimeSpan.FromDays(30))
        {
            throw new ArgumentException("Departure date cannot be more than 30 days in the future");
        }
        if(request.DepartureDate < DateTime.UtcNow.Date)
        {
            throw new ArgumentException("Departure date cannot be in the past");
        }

        if (request.IsDirectRoute)
        {
            return await GetDirectRoutes(fromStation, toStation, request.DepartureDate);
        }
        else
        {
            return await GetComplexRoutes(fromStation, toStation, request.DepartureDate);
        }
    }
    
    private async Task<IEnumerable<ComplexRouteDto>> GetDirectRoutes(Station fromStation, Station toStation, DateTime departureDate)
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

    private async Task<DirectRouteDto> CreateDirectRouteDto(IEnumerable<ConcreteRouteSegment> segments)
    {
        var segmentsList = segments.OrderBy(s => s.ConcreteDepartureDate).ToList();
        
        var startAbstractSegment = await abstractRouteSegmentRepository.GetByIdAsync(segmentsList[0].AbstractSegmentId);
        var endAbstractSegment = await abstractRouteSegmentRepository.GetByIdAsync(segmentsList[^1].AbstractSegmentId);
        
        if (endAbstractSegment == null || startAbstractSegment == null)
            throw new Exception("abstract segment not found");
        if (endAbstractSegment.SegmentNumber - startAbstractSegment.SegmentNumber + 1 != segments.Count())
            throw new Exception("given IEnumerable of segments doesn't make a correct sequence");

        var request = MapInfoRouteSegmentSearch(segmentsList[0].ConcreteRouteId, startAbstractSegment.SegmentNumber,
            endAbstractSegment.SegmentNumber);
        
        var availableSeats = await carriageSeatService.GetAvailableSeatsAmountAsync(request);
        var costRange = await priceCalculationService.GetRoutePriceRangeAsync(request);
        if (costRange == null)
            throw new Exception($"failed to get price range for route {request.ConcreteRouteId}");
        
        var directRouteDto = new DirectRouteDto
        {
            DepartureDate = segmentsList[0].ConcreteDepartureDate,
            ArrivalDate = segmentsList[^1].ConcreteArrivalDate,
            ConcreteRouteId = segmentsList[0].ConcreteRouteId,
            StartSegmentNumber = startAbstractSegment.SegmentNumber,
            EndSegmentNumber = endAbstractSegment.SegmentNumber,
            MaximumCost = costRange.MaximumPrice,
            MinimalCost = costRange.MinimalPrice,
            AvailableSeats = availableSeats,
            TimeInTransit = segmentsList[0].ConcreteDepartureDate - segmentsList[^1].ConcreteArrivalDate
        };
        return directRouteDto;
    }
    
    private async Task<IEnumerable<ComplexRouteDto>> GetComplexRoutes(Station fromStation, Station toStation, DateTime departureDate)
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

        var complexRouteDtos = new List<ComplexRouteDto>();
        
        // 3. For every concrete route segment try to find the shortest route to toStation

        var concreteSegmentsCash = new Dictionary<Guid, List<ConcreteRouteSegment>>();
        var abstractSegmentCash = new Dictionary<Guid, AbstractRouteSegment>();
        
        foreach (var startRouteSegment in startConcreteRouteSegments)
        {
            // 4. Use Dijkstra algo to find best route no longer than 72 hours

            var stationsInfo = new Dictionary<Guid, StationInfoNode>();
            var q = new PriorityQueue<ValuePathConditionNode, KeyPathConditionNode>(new KeyPathComparer());
            
            stationsInfo[fromStation.Id] = new StationInfoNode(startRouteSegment.ConcreteDepartureDate,
                startRouteSegment, Guid.Empty, 0);
            
            q.Enqueue(new ValuePathConditionNode(fromStation.Id, startRouteSegment.ConcreteRouteId, startRouteSegment.ConcreteDepartureDate, 0), new KeyPathConditionNode(startRouteSegment.ConcreteDepartureDate, 0));
            while (q.Count != 0)
            {
                var currentPathConditionNode = q.Dequeue();
                var currentArrivalDatetime = currentPathConditionNode.ArrivalDate;
                var currentAmountOfTransfers = currentPathConditionNode.AmountOfTransfers;
                var currentStationId = currentPathConditionNode.StationId;
                var currentRouteId = currentPathConditionNode.RouteId;

                if (stationsInfo.TryGetValue(currentStationId, out var stationsInfoValue))
                {
                    if (stationsInfoValue.ArrivalDateTime < currentArrivalDatetime || (stationsInfoValue.ArrivalDateTime == currentArrivalDatetime && stationsInfoValue.AmountOfTransfers < currentAmountOfTransfers))
                    {
                        continue;
                    }

                    if (currentStationId == fromStation.Id)
                    {
                        var abstractSegment =
                            await abstractRouteSegmentRepository.GetByIdAsync(startRouteSegment.AbstractSegmentId);
                        abstractSegmentCash[abstractSegment!.Id] = abstractSegment;
                        concreteSegmentsCash[currentStationId] = new List<ConcreteRouteSegment>{startRouteSegment};
                    }
                    if (!concreteSegmentsCash.ContainsKey(currentStationId))
                    {
                        var currentAbstractSegments =
                            await abstractRouteSegmentRepository
                                .GetAbstractSegmentsByFromStationAsync(currentStationId);
                        foreach (var abstractSegment in currentAbstractSegments)
                        {
                            abstractSegmentCash[abstractSegment.Id] = abstractSegment;
                            var currentConcreteSegments =
                                await concreteRouteSegmentRepository.GetConcreteSegmentsByAbstractSegmentIdAsync(
                                    abstractSegment.Id);
                            var actualConcreteSegments =
                                currentConcreteSegments.Where(s =>
                                    s.ConcreteDepartureDate.Date >= departureDate.Date &&
                                    s.ConcreteArrivalDate <= departureDate.Date.AddDays(3));

                            if (concreteSegmentsCash.ContainsKey(currentStationId))
                                concreteSegmentsCash[currentStationId].AddRange(actualConcreteSegments);
                            else
                                concreteSegmentsCash[currentStationId]=actualConcreteSegments.ToList();
                        }
                    }

                    foreach (var currentNextSegment in concreteSegmentsCash[currentStationId])
                    {
                        var nextArrivalDatetime = new DateTime();
                        var nextAmountOfTransfers = 0;
                        var nextStationId = Guid.Empty;
                        var nextRouteId = Guid.Empty;
                        
                        // look at next segment of path
                        if (currentNextSegment.ConcreteRouteId == currentRouteId)
                        {
                            // continue current route
                            nextArrivalDatetime = currentNextSegment.ConcreteArrivalDate;
                            nextAmountOfTransfers = currentAmountOfTransfers;
                            nextStationId = abstractSegmentCash[currentNextSegment.AbstractSegmentId].ToStationId;
                            nextRouteId = currentRouteId;
                        }
                        else
                        {
                            if (currentNextSegment.ConcreteDepartureDate - currentArrivalDatetime <
                                TimeSpan.FromMinutes(15))
                            {
                                continue;
                            }
                            
                            nextArrivalDatetime = currentNextSegment.ConcreteArrivalDate;
                            nextAmountOfTransfers = currentAmountOfTransfers + 1;
                            nextStationId = abstractSegmentCash[currentNextSegment.AbstractSegmentId].ToStationId;
                            nextRouteId = currentNextSegment.ConcreteRouteId;
                        }
                        
                        // update answer for next station and add to queue
                        if(stationsInfo.ContainsKey(nextStationId) && stationsInfo[nextStationId].ArrivalDateTime < nextArrivalDatetime)
                            continue;
                        if(stationsInfo.ContainsKey(nextStationId) && stationsInfo[nextStationId].ArrivalDateTime == nextArrivalDatetime && stationsInfo[nextStationId].AmountOfTransfers < nextAmountOfTransfers)
                            continue;

                        stationsInfo[nextStationId] = new StationInfoNode(nextArrivalDatetime, currentNextSegment,
                            currentStationId, nextAmountOfTransfers);
                        
                        Debug.WriteLine($"stationInfo for station {nextStationId}: nextArrivalTime: {nextArrivalDatetime}, nextSegment: {currentNextSegment.ConcreteDepartureDate} - {currentNextSegment.ConcreteArrivalDate} from stationId {currentStationId} transfers: {nextAmountOfTransfers}");
                        
                        q.Enqueue(new ValuePathConditionNode(nextStationId, nextRouteId, nextArrivalDatetime, nextAmountOfTransfers), new KeyPathConditionNode(nextArrivalDatetime, nextAmountOfTransfers));
                    }
                    
                }
                else
                {
                    throw new Exception($"element in dictionary with key {currentStationId} wasn't found ");
                }
            }
            
            Debug.WriteLine($"end of algo for segment {startRouteSegment.ConcreteDepartureDate} {startRouteSegment.ConcreteArrivalDate}");
            // 5. map root
            if (stationsInfo.ContainsKey(toStation.Id))
            {
                // route was found
                
                var directRouteDtos = new List<DirectRouteDto>();
                var currentStationId = toStation.Id;
                var currentRouteId = Guid.Empty;
                var segmentsForLastRoute = new List<ConcreteRouteSegment>();
                
                while (currentStationId != fromStation.Id)
                {
                    var currentStationInfo = stationsInfo[currentStationId];
                    if (currentStationInfo.ConcreteSegment.ConcreteRouteId != currentRouteId)
                    {
                        if (segmentsForLastRoute.Count != 0)
                        {
                            var directRouteDto = await CreateDirectRouteDto(segmentsForLastRoute);
                            directRouteDtos.Add(directRouteDto);
                        }
                        segmentsForLastRoute.Clear();
                    }
                    segmentsForLastRoute.Add(currentStationInfo.ConcreteSegment);
                    currentRouteId = currentStationInfo.ConcreteSegment.ConcreteRouteId;
                    currentStationId = currentStationInfo.PreviousStationId;
                }
                if (segmentsForLastRoute.Count != 0)
                {
                    var directRouteDto = await CreateDirectRouteDto(segmentsForLastRoute);
                    directRouteDtos.Add(directRouteDto);
                }
                var complexRouteDto = CreateComplexRouteDto(directRouteDtos);
                complexRouteDtos.Add(complexRouteDto);
            }
        }

        return complexRouteDtos;
    }
}

class KeyPathComparer : Comparer<KeyPathConditionNode>
{
    public override int Compare(KeyPathConditionNode x, KeyPathConditionNode y)
    {
        if (x.ArrivalDate != y.ArrivalDate)
        {
            var result = DateTime.Compare(x.ArrivalDate, y.ArrivalDate);
            return result;
        }

        return x.AmountOfTransfers - y.AmountOfTransfers;
    }
}

struct KeyPathConditionNode(DateTime arrivalDate, int amountOfTransfers)
{
    public DateTime ArrivalDate = arrivalDate;
    public int AmountOfTransfers = amountOfTransfers;
}
struct ValuePathConditionNode(Guid stationId, Guid routeId, DateTime arrivalDate, int amountOfTransfers)
{
    public Guid StationId = stationId;
    public Guid RouteId = routeId;
    public DateTime ArrivalDate = arrivalDate;
    public int AmountOfTransfers = amountOfTransfers;
}

struct StationInfoNode(DateTime arrivalDateTime, ConcreteRouteSegment concreteSegment, Guid previousStationId, int amountOfTransfers)
{
    public DateTime ArrivalDateTime = arrivalDateTime;
    public ConcreteRouteSegment ConcreteSegment = concreteSegment;
    
    public Guid PreviousStationId = previousStationId;
    public int AmountOfTransfers = amountOfTransfers;
}