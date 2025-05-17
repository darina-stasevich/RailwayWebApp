using System.Diagnostics;
using RailwayApp.Application.Models;
using RailwayApp.Application.Models.Dto;
using RailwayApp.Domain;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces.IRepositories;
using RailwayApp.Domain.Interfaces.IServices;

namespace RailwayApp.Application.Services;

public class RouteSearchService(
    IStationRepository stationRepository,
    IAbstractRouteSegmentRepository abstractRouteSegmentRepository,
    IConcreteRouteSegmentRepository concreteRouteSegmentRepository,
    IPriceCalculationService priceCalculationService,
    ICarriageSeatService carriageSeatService) : IRouteSearchService
{
    public async Task<IEnumerable<ComplexRouteDto>> GetRoutesAsync(RouteSearchRequest request)
    {
        var fromStation = await stationRepository.GetByIdAsync(request.FromStationId);
        if (fromStation == null) throw new StationNotFoundException(request.FromStationId);
        var toStation = await stationRepository.GetByIdAsync(request.ToStationId);
        if (toStation == null) throw new StationNotFoundException(request.ToStationId);
        if (request.DepartureDate.Date - DateTime.UtcNow.Date > TimeSpan.FromDays(30))
            throw new ArgumentException("Departure date cannot be more than 30 days in the future");
        if (request.DepartureDate.Date < DateTime.UtcNow.Date)
            throw new ArgumentException("Departure date cannot be in the past");

        if (request.IsDirectRoute)
            return await GetDirectRoutes(fromStation, toStation, request.DepartureDate);
        return await GetComplexRoutes(fromStation, toStation, request.DepartureDate);
    }

    private InfoRouteSegmentSearchDto MapInfoRouteSegmentSearch(Guid routeId, int startSegmentNumber,
        int endSegmentNumber)
    {
        return new InfoRouteSegmentSearchDto
        {
            ConcreteRouteId = routeId,
            EndSegmentNumber = endSegmentNumber,
            StartSegmentNumber = startSegmentNumber
        };
    }

    private InfoRouteSegmentSearchPerCarriageDto MapInfoRouteSegmentPerCarriageSearch(Guid routeId,
        int startSegmentNumber, int endSegmentNumber, Guid carriageTemplateId)
    {
        return new InfoRouteSegmentSearchPerCarriageDto
        {
            ConcreteRouteId = routeId,
            EndSegmentNumber = endSegmentNumber,
            StartSegmentNumber = startSegmentNumber,
            CarriageTemplateId = carriageTemplateId
        };
    }

    private async Task<IEnumerable<ComplexRouteDto>> GetDirectRoutes(Station fromStation, Station toStation,
        DateTime departureDate)
    {
        TimeZoneInfo localTimeZone;
        try
        {
            localTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Moscow");
        }
        catch (Exception)
        {
            localTimeZone = TimeZoneInfo.CreateCustomTimeZone("CustomLocalTimeZone_UTC+3", TimeSpan.FromHours(3),
                "Local Time (UTC+3)", "Local Time (UTC+3)");
        }

        var departureDateAsUtc = departureDate.ToUniversalTime();

        // local time of request
        var localEquivalentOfDepartureDate = TimeZoneInfo.ConvertTimeFromUtc(departureDateAsUtc, localTimeZone);
        // local day
        var startOfLocalDayInLocalZone = localEquivalentOfDepartureDate.Date;
        // local datetime in utc +0
        var startOfRequestedDayUtc = TimeZoneInfo.ConvertTimeToUtc(startOfLocalDayInLocalZone, localTimeZone);
        // local end of the day
        var endOfRequestedDayUtc = startOfRequestedDayUtc.AddDays(1);
        var endOfSegmentsRequestedDayUtc = startOfRequestedDayUtc.AddDays(3);

        // 1. Get concrete route segments that departure from fromStation
        var allStartConcreteRouteSegments =
            await concreteRouteSegmentRepository.GetConcreteSegmentsByFromStationAsync(fromStation.Id);
        // 2. Get concrete route segments that departure in given day after current time
        var startConcreteRouteSegments = allStartConcreteRouteSegments.Where(segment =>
            {
                var segmentDepartureInUtc = segment.ConcreteDepartureDate;
                var isOnRequestedDate = segmentDepartureInUtc >= startOfRequestedDayUtc &&
                                        segmentDepartureInUtc < endOfRequestedDayUtc;
                var isAfterCurrentTime = segmentDepartureInUtc > DateTime.UtcNow;
                return isOnRequestedDate && isAfterCurrentTime;
            })
            .ToList();


        // 3. Get concrete route id
        var startConcreteRouteIds = new HashSet<Guid>(
            startConcreteRouteSegments.Select(s => s.ConcreteRouteId)
        );

        // 4. Find concrete route segments that end in toStation
        var allEndConcreteRouteSegments =
            await concreteRouteSegmentRepository.GetConcreteSegmentsByToStationAsync(toStation.Id);

        // 5. Find concrete end route segments that are in start concrete route segments
        var endConcreteRouteSegments =
            allEndConcreteRouteSegments.Where(x => startConcreteRouteIds.Contains(x.ConcreteRouteId));

        // 6. get concrete routes that has both start and end segments
        var bothConcreteRouteIds = new HashSet<Guid>(
            endConcreteRouteSegments.Select(s => s.ConcreteRouteId));

        // 7. check that start segment has number less than end segment
        // 8. for valid routes calculate cost and search for available seats
        // 9. generate dto

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
                startConcreteSegmentsDict[startRouteDict[routeId]].SegmentNumber;
            var endNumber =
                endConcreteSegmentsDict[endRouteDict[routeId]].SegmentNumber;
            var startSegment = startConcreteSegmentsDict[startRouteDict[routeId]];
            var endSegment = endConcreteSegmentsDict[endRouteDict[routeId]];
            if (startNumber <= endNumber)
            {
                var infoRouteSegmentDto = MapInfoRouteSegmentSearch(routeId, startNumber, endNumber);
                var costRange = await priceCalculationService.GetRoutePriceRangeAsync(infoRouteSegmentDto);
                var availableSeats = await carriageSeatService.GetAvailableSeatsAmountAsync(infoRouteSegmentDto);

                var routeDto = new DirectRouteDto
                {
                    ConcreteRouteId = routeId,
                    StartSegmentNumber = startNumber,
                    EndSegmentNumber = endNumber,
                    FromStationId = fromStation.Id,
                    ToStationId = toStation.Id,
                    DepartureDate = startSegment.ConcreteDepartureDate,
                    ArrivalDate = endSegment.ConcreteArrivalDate,
                    TimeInTransit = endSegment.ConcreteArrivalDate - startSegment.ConcreteDepartureDate,
                    MinimalCost = costRange.MinimalPrice,
                    MaximumCost = costRange.MaximumPrice,
                    AvailableSeats = availableSeats
                };

                var complexRoute = CreateComplexRouteDto(new List<DirectRouteDto> { routeDto });
                directRoutes.Add(complexRoute);
            }
        }

        return directRoutes;
    }

    private ComplexRouteDto CreateComplexRouteDto(IEnumerable<DirectRouteDto> directRoutes)
    {
        var directRouteDtos = directRoutes.OrderBy(r => r.DepartureDate).ToList();
        var complexRoute = new ComplexRouteDto
        {
            DirectRoutes = directRouteDtos,
            MinimalTotalCost = directRouteDtos.Sum(r => r.MinimalCost),
            MaximumTotalCost = directRouteDtos.Sum(r => r.MaximumCost),
            DepartureDate = directRouteDtos.Min(r => r.DepartureDate),
            ArrivalDate = directRouteDtos.Max(r => r.ArrivalDate),
            TotalDuration = directRouteDtos.Max(r => r.ArrivalDate) - directRouteDtos.Min(r => r.DepartureDate)
        };
        return complexRoute;
    }

    private async Task<DirectRouteDto> CreateDirectRouteDto(IEnumerable<ConcreteRouteSegment> segments)
    {
        var segmentsList = segments.OrderBy(s => s.ConcreteDepartureDate).ToList();

        var startAbstractSegment = await abstractRouteSegmentRepository.GetByIdAsync(segmentsList[0].AbstractSegmentId);
        var endAbstractSegment = await abstractRouteSegmentRepository.GetByIdAsync(segmentsList[^1].AbstractSegmentId);

        if (startAbstractSegment == null)
            throw new AbstractRouteSegmentNotFoundException(segmentsList[0].AbstractSegmentId);
        if (endAbstractSegment == null)
            throw new AbstractRouteSegmentNotFoundException(segmentsList[^1].AbstractSegmentId);
        if (endAbstractSegment.SegmentNumber - startAbstractSegment.SegmentNumber + 1 != segments.Count())
            throw new RouteSearchServicePathCreatingFailedException("given segments not make a valid path");

        var request = MapInfoRouteSegmentSearch(segmentsList[0].ConcreteRouteId, startAbstractSegment.SegmentNumber,
            endAbstractSegment.SegmentNumber);

        var availableSeats = await carriageSeatService.GetAvailableSeatsAmountAsync(request);
        var costRange = await priceCalculationService.GetRoutePriceRangeAsync(request);

        var directRouteDto = new DirectRouteDto
        {
            DepartureDate = segmentsList[0].ConcreteDepartureDate,
            ArrivalDate = segmentsList[^1].ConcreteArrivalDate,
            ConcreteRouteId = segmentsList[0].ConcreteRouteId,
            StartSegmentNumber = startAbstractSegment.SegmentNumber,
            EndSegmentNumber = endAbstractSegment.SegmentNumber,
            FromStationId = startAbstractSegment.FromStationId,
            ToStationId = endAbstractSegment.ToStationId,
            MaximumCost = costRange.MaximumPrice,
            MinimalCost = costRange.MinimalPrice,
            AvailableSeats = availableSeats,
            TimeInTransit = segmentsList[0].ConcreteDepartureDate - segmentsList[^1].ConcreteArrivalDate
        };
        return directRouteDto;
    }

    private async Task<IEnumerable<ComplexRouteDto>> GetComplexRoutes(Station fromStation, Station toStation,
        DateTime departureDate)
    {
        TimeZoneInfo localTimeZone;
        try
        {
            localTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Moscow");
        }
        catch (Exception)
        {
            localTimeZone = TimeZoneInfo.CreateCustomTimeZone("CustomLocalTimeZone_UTC+3", TimeSpan.FromHours(3),
                "Local Time (UTC+3)", "Local Time (UTC+3)");
        }

        var departureDateAsUtc = departureDate.ToUniversalTime();

        // local time of request
        var localEquivalentOfDepartureDate = TimeZoneInfo.ConvertTimeFromUtc(departureDateAsUtc, localTimeZone);
        // local day
        var startOfLocalDayInLocalZone = localEquivalentOfDepartureDate.Date;
        // local datetime in utc +0
        var startOfRequestedDayUtc = TimeZoneInfo.ConvertTimeToUtc(startOfLocalDayInLocalZone, localTimeZone);
        // local end of the day
        var endOfRequestedDayUtc = startOfRequestedDayUtc.AddDays(1);
        var endOfSegmentsRequestedDayUtc = startOfRequestedDayUtc.AddDays(3);

        // 1. Get concrete route segments that departure from fromStation
        var allStartConcreteRouteSegments =
            await concreteRouteSegmentRepository.GetConcreteSegmentsByFromStationAsync(fromStation.Id);
        // 2. Get concrete route segments that departure in given day after current time
        var startConcreteRouteSegments = allStartConcreteRouteSegments.Where(segment =>
            {
                var segmentDepartureInUtc = segment.ConcreteDepartureDate;
                var isOnRequestedDate = segmentDepartureInUtc >= startOfRequestedDayUtc &&
                                        segmentDepartureInUtc < endOfRequestedDayUtc;
                var isAfterCurrentTime = segmentDepartureInUtc > DateTime.UtcNow;
                return isOnRequestedDate && isAfterCurrentTime;
            })
            .ToList();

        var complexRouteDtos = new List<ComplexRouteDto>();

        // 3. For every concrete route segment try to find the shortest route to toStation

        var concreteSegmentsCash = new Dictionary<Guid, List<ConcreteRouteSegment>>();

        foreach (var startRouteSegment in startConcreteRouteSegments)
        {
            // 4. Use Dijkstra algo to find best route no longer than 72 hours

            var stationsInfo = new Dictionary<Guid, StationInfoNode>();
            var q = new PriorityQueue<ValuePathConditionNode, KeyPathConditionNode>(new KeyPathComparer());

            stationsInfo[fromStation.Id] = new StationInfoNode(startRouteSegment.ConcreteDepartureDate,
                startRouteSegment, Guid.Empty, 0);

            q.Enqueue(
                new ValuePathConditionNode(fromStation.Id, startRouteSegment.ConcreteRouteId,
                    startRouteSegment.ConcreteDepartureDate, 0),
                new KeyPathConditionNode(startRouteSegment.ConcreteDepartureDate, 0));
            while (q.Count != 0)
            {
                var currentPathConditionNode = q.Dequeue();
                var currentArrivalDatetime = currentPathConditionNode.ArrivalDate;
                var currentAmountOfTransfers = currentPathConditionNode.AmountOfTransfers;
                var currentStationId = currentPathConditionNode.StationId;
                var currentRouteId = currentPathConditionNode.RouteId;

                if (stationsInfo.TryGetValue(currentStationId, out var stationsInfoValue))
                {
                    if (stationsInfoValue.ArrivalDateTime < currentArrivalDatetime ||
                        (stationsInfoValue.ArrivalDateTime == currentArrivalDatetime &&
                         stationsInfoValue.AmountOfTransfers < currentAmountOfTransfers)) continue;

                    if (currentStationId == fromStation.Id)
                        concreteSegmentsCash[currentStationId] = new List<ConcreteRouteSegment> { startRouteSegment };
                    if (!concreteSegmentsCash.ContainsKey(currentStationId))
                    {
                        var allConcreteSegmentsForStation =
                            await concreteRouteSegmentRepository
                                .GetConcreteSegmentsByFromStationAsync(currentStationId);

                        var actualConcreteSegments = allConcreteSegmentsForStation
                            .Where(s => s.ConcreteDepartureDate >= startOfRequestedDayUtc
                                        && s.ConcreteArrivalDate <= endOfSegmentsRequestedDayUtc)
                            .ToList();

                        if (concreteSegmentsCash.ContainsKey(currentStationId))
                            concreteSegmentsCash[currentStationId].AddRange(actualConcreteSegments);
                        else
                            concreteSegmentsCash[currentStationId] = actualConcreteSegments.ToList();
                    }

                    foreach (var currentNextSegment in concreteSegmentsCash[currentStationId])
                    {
                        if (currentNextSegment.ConcreteDepartureDate < currentArrivalDatetime)
                            continue;

                        // look at next segment of path
                        Guid nextRouteId;
                        Guid nextStationId;
                        var nextAmountOfTransfers = 0;
                        DateTime nextArrivalDatetime;

                        if (currentNextSegment.ConcreteRouteId == currentRouteId)
                        {
                            // continue current route
                            nextArrivalDatetime = currentNextSegment.ConcreteArrivalDate;
                            nextAmountOfTransfers = currentAmountOfTransfers;
                            nextStationId = currentNextSegment.ToStationId;
                            nextRouteId = currentRouteId;
                        }
                        else
                        {
                            if (currentNextSegment.ConcreteDepartureDate - currentArrivalDatetime <
                                TimeSpan.FromMinutes(15))
                                continue;

                            nextArrivalDatetime = currentNextSegment.ConcreteArrivalDate;
                            nextAmountOfTransfers = currentAmountOfTransfers + 1;
                            nextStationId = currentNextSegment.ToStationId;
                            nextRouteId = currentNextSegment.ConcreteRouteId;
                        }

                        // update answer for next station and add to queue
                        if (stationsInfo.ContainsKey(nextStationId) &&
                            stationsInfo[nextStationId].ArrivalDateTime < nextArrivalDatetime)
                            continue;
                        if (stationsInfo.ContainsKey(nextStationId) &&
                            stationsInfo[nextStationId].ArrivalDateTime == nextArrivalDatetime &&
                            stationsInfo[nextStationId].AmountOfTransfers < nextAmountOfTransfers)
                            continue;

                        stationsInfo[nextStationId] = new StationInfoNode(nextArrivalDatetime, currentNextSegment,
                            currentStationId, nextAmountOfTransfers);

                        Debug.WriteLine(
                            $"stationInfo for station {nextStationId}: nextArrivalTime: {nextArrivalDatetime}, nextSegment: {currentNextSegment.ConcreteDepartureDate} - {currentNextSegment.ConcreteArrivalDate} from stationId {currentStationId} transfers: {nextAmountOfTransfers}");

                        q.Enqueue(
                            new ValuePathConditionNode(nextStationId, nextRouteId, nextArrivalDatetime,
                                nextAmountOfTransfers),
                            new KeyPathConditionNode(nextArrivalDatetime, nextAmountOfTransfers));
                    }

                    Debug.WriteLine($"{currentStationId} arrival time: {currentArrivalDatetime}");
                    foreach (var station in stationsInfo)
                        Debug.WriteLine(
                            $"{station.Key} - {station.Value.PreviousStationId} - arrival at {station.Value.ArrivalDateTime} + \n" +
                            $"from {station.Value.ConcreteSegment.FromStationId} ({station.Value.ConcreteSegment.ConcreteDepartureDate}) to {station.Value.ConcreteSegment.ToStationId} ({station.Value.ConcreteSegment.ConcreteArrivalDate})");
                }
                else
                {
                    throw new Exception($"element in dictionary with key {currentStationId} wasn't found ");
                }
            }

            Debug.WriteLine(
                $"end of algo for segment {startRouteSegment.ConcreteDepartureDate} {startRouteSegment.ConcreteArrivalDate}");
            // 5. map root
            if (stationsInfo.ContainsKey(toStation.Id))
            {
                // route was found

                var directRouteDtos = new List<DirectRouteDto>();
                var currentStationId = toStation.Id;
                var currentRouteId = Guid.Empty;
                var segmentsForLastRoute = new List<ConcreteRouteSegment>();
                foreach (var station in stationsInfo)
                    Debug.WriteLine(
                        $"{station.Key} - {station.Value.PreviousStationId} - arrival at {station.Value.ArrivalDateTime} + \n" +
                        $"from {station.Value.ConcreteSegment.FromStationId} ({station.Value.ConcreteSegment.ConcreteDepartureDate}) to {station.Value.ConcreteSegment.ToStationId} ({station.Value.ConcreteSegment.ConcreteArrivalDate})");

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

internal class KeyPathComparer : Comparer<KeyPathConditionNode>
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

internal struct KeyPathConditionNode(DateTime arrivalDate, int amountOfTransfers)
{
    public DateTime ArrivalDate = arrivalDate;
    public int AmountOfTransfers = amountOfTransfers;
}

internal struct ValuePathConditionNode(Guid stationId, Guid routeId, DateTime arrivalDate, int amountOfTransfers)
{
    public Guid StationId = stationId;
    public Guid RouteId = routeId;
    public DateTime ArrivalDate = arrivalDate;
    public int AmountOfTransfers = amountOfTransfers;
}

internal struct StationInfoNode(
    DateTime arrivalDateTime,
    ConcreteRouteSegment concreteSegment,
    Guid previousStationId,
    int amountOfTransfers)
{
    public DateTime ArrivalDateTime = arrivalDateTime;
    public ConcreteRouteSegment ConcreteSegment = concreteSegment;

    public Guid PreviousStationId = previousStationId;
    public int AmountOfTransfers = amountOfTransfers;
}