using MongoDB.Driver;
using RailwayApp.Application.Models;
using RailwayApp.Domain;
using RailwayApp.Domain.Interfaces.IRepositories;
using RailwayApp.Domain.Interfaces.IServices;

namespace RailwayApp.Application.Services;

public class ScheduleService(IUnitOfWork unitOfWork) : IScheduleService
{
    public async Task<ScheduleDto> GetScheduleAsync(Guid concreteRouteId)
    {
        // 1. Get concrete route
        var concreteRoute = await unitOfWork.ConcreteRoutes.GetByIdAsync(concreteRouteId);
        if (concreteRoute == null) throw new ConcreteRouteNotFoundException(concreteRouteId);

        // 2. Get abstract route
        var abstractRoute = await unitOfWork.AbstractRoutes.GetByIdAsync(concreteRoute.AbstractRouteId);
        if (abstractRoute == null) throw new AbstractRouteNotFoundException(concreteRoute.AbstractRouteId);

        // 3. Get concrete segments by concreteRouteId
        var concreteRouteSegments =
            (await unitOfWork.ConcreteRouteSegments.GetConcreteSegmentsByConcreteRouteIdAsync(concreteRouteId)).ToList();
        if (concreteRouteSegments == null || concreteRouteSegments.Count == 0)
            throw new ConcreteRouteSegmentsNotFoundException(concreteRouteId);

        // 4. Get stations

        var stationIds = concreteRouteSegments.Select(x => x.ToStationId)
            .Append(concreteRouteSegments[0].FromStationId);

        var stations = await unitOfWork.Stations.GetByIdsAsync(stationIds.ToList());
        if (stations == null || stations.Count() == 0) throw new StationExceptions("stations for given ids not found");
        var stationsDictionary = stations.ToDictionary(x => x.Id, v => v.Name);

        // 6. Create segment schedule dtos
        var segmentScheduleDtos = new List<SegmentScheduleDto>();
        foreach (var concreteRouteSegment in concreteRouteSegments)
        {
            var segmentScheduleDto = new SegmentScheduleDto
            {
                DepartureDate = concreteRouteSegment.ConcreteDepartureDate,
                ArrivalDate = concreteRouteSegment.ConcreteArrivalDate,
                FromStation = stationsDictionary[concreteRouteSegment.FromStationId],
                ToStation = stationsDictionary[concreteRouteSegment.ToStationId]
            };
            segmentScheduleDtos.Add(segmentScheduleDto);
        }

        var scheduleDto = new ScheduleDto
        {
            ConcreteRouteId = concreteRouteId,
            DepartureDate = DateOnly.FromDateTime(concreteRoute.RouteDepartureDate),
            TrainNumber = abstractRoute.TrainNumber,
            Segments = segmentScheduleDtos
        };

        return scheduleDto;
    }

    public async Task<DateTime> GetDepartureDateForSegment(Guid concreteRouteId, int segmentNumber,
        IClientSessionHandle? session = null)
    {
        var concreteRouteSegments =
            await unitOfWork.ConcreteRouteSegments.GetConcreteSegmentsByConcreteRouteIdAsync(concreteRouteId, session);

        var departureDate = concreteRouteSegments.Where(s => s.SegmentNumber == segmentNumber)
            .Select(s => s.ConcreteDepartureDate).FirstOrDefault();
        return departureDate;
    }

    public async Task<DateTime> GetArrivalDateForSegment(Guid concreteRouteId, int segmentNumber,
        IClientSessionHandle? session = null)
    {
        var concreteRouteSegments =
            await unitOfWork.ConcreteRouteSegments.GetConcreteSegmentsByConcreteRouteIdAsync(concreteRouteId, session);

        var arrivalDate = concreteRouteSegments.Where(s => s.SegmentNumber == segmentNumber)
            .Select(s => s.ConcreteArrivalDate).FirstOrDefault();
        return arrivalDate;
    }
}