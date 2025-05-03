using RailwayApp.Application.Models;
using RailwayApp.Domain.Interfaces.IRepositories;
using RailwayApp.Domain.Interfaces.IServices;

namespace RailwayApp.Application.Services;

public class ScheduleService(IConcreteRouteSegmentRepository concreteRouteSegmentRepository,
    IAbstractRouteSegmentRepository abstractRouteSegmentRepository,
    IAbstractRouteRepository abstractRouteRepository,
    IConcreteRouteRepository concreteRouteRepository,
    IStationRepository stationRepository) : IScheduleService
{
    public async Task<ScheduleDto> GetScheduleAsync(Guid concreteRouteId)
    {
        // 1. Get concrete route
        var concreteRoute = await concreteRouteRepository.GetByIdAsync(concreteRouteId);
        if(concreteRoute == null)
        {
            throw new ArgumentException("No concrete route found.");
        }
        
        // 2. Get abstract route
        var abstractRoute = await abstractRouteRepository.GetByIdAsync(concreteRoute.AbstractRouteId);
        if(abstractRoute == null)
        {
            throw new ArgumentException("No abstract route found.");
        }
        
        // 3. Get concrete segments by concreteRouteId
        var concreteRouteSegments = await concreteRouteSegmentRepository.GetConcreteSegmentsByConcreteRouteIdAsync(concreteRouteId);
        if (concreteRouteSegments == null || concreteRouteSegments.Count == 0)
        {
            throw new Exception("No concrete route segments found.");
        }
        var sortedRouteSegments = concreteRouteSegments.OrderBy(x => x.ConcreteDepartureDate).ToList();

        // 4. Get abstract segments by abstractRouteId
        var abstractRouteSegments = await abstractRouteSegmentRepository.GetAbstractSegmentsByRouteIdAsync(abstractRoute.Id);
        if (abstractRouteSegments == null || abstractRouteSegments.Count == 0)
        {
            throw new Exception("No abstract route segments found.");
        }
        
        var abstractRouteSegmentsDictionary = abstractRouteSegments.ToDictionary(x => x.Id);
        var stationIds = abstractRouteSegments.Select(x => x.ToStationId).Append(abstractRouteSegments[0].FromStationId);

        // 5. Get stations
        var stations = await stationRepository.GetByIdsAsync(stationIds.ToList());
        if (stations == null || stations.Count == 0)
        {
            throw new Exception("No stations found.");
        }
        var stationsDictionary = stations.ToDictionary(x => x.Id, v => v.Name);

        // 6. Create segment schedule dtos
        var segmentScheduleDtos = new List<SegmentScheduleDto>();
        foreach (var concreteRouteSegment in concreteRouteSegments)
        {
            var segmentScheduleDto = new SegmentScheduleDto
            {
                DepartureDate = concreteRouteSegment.ConcreteDepartureDate,
                ArrivalDate = concreteRouteSegment.ConcreteArrivalDate,
                FromStation = stationsDictionary[abstractRouteSegmentsDictionary[concreteRouteSegment.AbstractSegmentId].FromStationId],
                ToStation = stationsDictionary[abstractRouteSegmentsDictionary[concreteRouteSegment.AbstractSegmentId].ToStationId]
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
}