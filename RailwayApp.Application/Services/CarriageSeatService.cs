using System.Collections;
using RailwayApp.Application.Models.Dto;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces.IRepositories;
using RailwayApp.Domain.Interfaces.IServices;

namespace RailwayApp.Application.Services;

public class CarriageSeatService(IConcreteRouteRepository concreteRouteRepository,
    IAbstractRouteRepository abstractRouteRepository,
    IAbstractRouteSegmentRepository abstractRouteSegmentRepository,
    IConcreteRouteSegmentRepository concreteRouteSegmentRepository,
    ICarriageAvailabilityRepository carriageAvailabilityRepository
    ) : ICarriageSeatService
{
    private InfoRouteSegmentSearchDto MapInfoRouteSegment(InfoRouteSegmentSearchPerCarriageDto dto)
    {
        return new InfoRouteSegmentSearchDto{
            ConcreteRouteId = dto.ConcreteRouteId,
            StartSegmentNumber = dto.StartSegmentNumber,
            EndSegmentNumber = dto.EndSegmentNumber
        };
    }
    private async Task<IEnumerable<IGrouping<Guid,CarriageAvailability>>?> GetAllCarriageAvailabilitiesForRouteAsync(InfoRouteSegmentSearchDto dto)
    {
        // 1. Get concrete route    
        var concreteRoute = await concreteRouteRepository.GetByIdAsync(dto.ConcreteRouteId);
        if (concreteRoute == null)
        {
            throw new Exception("Concrete route not found");
        }

        // 2. Get relevant abstract route
        var abstractRoute = await abstractRouteRepository.GetByIdAsync(concreteRoute.AbstractRouteId);
        if (abstractRoute == null)
        {
            throw new Exception("Abstract route not found");
        }
        
        // 3. Get relevant abstract route segments (has segment numbers)
        var abstractRouteSegments =
            await abstractRouteSegmentRepository.GetAbstractSegmentsByRouteIdAsync(abstractRoute.Id);
        var relevantAbstractRouteSegments = abstractRouteSegments
            .Where(s => s.SegmentNumber >= dto.StartSegmentNumber && s.SegmentNumber <= dto.EndSegmentNumber);
        var segmentsHashSet = new HashSet<Guid>(relevantAbstractRouteSegments.Select(s => s.Id));
        
        // 4. Get relevant concrete route segments
        var relevantConcreteRouteSegments =
            (await concreteRouteSegmentRepository.GetConcreteSegmentsByConcreteRouteIdAsync(dto.ConcreteRouteId))
            .Where(s => segmentsHashSet.Contains(s.AbstractSegmentId));

        // 5. get all availabilities for segments
        var allAvailabilities = new List<CarriageAvailability>();
        foreach (var routeSegment in relevantConcreteRouteSegments)
        {
            var carriageAvailabilities = await carriageAvailabilityRepository.GetByConcreteSegmentIdAsync(routeSegment.Id);
            allAvailabilities.AddRange(carriageAvailabilities);
        }
        
        // 6. Calculate seats that are free every segment in range
        var availabilityGroupedByCarriageTemplateId = allAvailabilities
            .GroupBy(a => a.CarriageTemplateId);

        return availabilityGroupedByCarriageTemplateId;
    }
    
    public async Task<int> GetAvailableSeatsAmountAsync(InfoRouteSegmentSearchDto dto)
    {
        var availabilityGroupedByCarriage = await GetAllCarriageAvailabilitiesForRouteAsync(dto);
        if (availabilityGroupedByCarriage == null)
        {
            throw new Exception("No carriages found found");
        }
        
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

    public async Task<Dictionary<Guid, int>> GetAvailableSeatCountsPerCarriageAsync(InfoRouteSegmentSearchDto dto)
    {
        var groupedCarriageAvailabilities = await GetAllCarriageAvailabilitiesForRouteAsync(dto);

        if (groupedCarriageAvailabilities == null)
        {
            throw new Exception("Carriage availabilities not found");
        }
        
        var availableSeatsPerCarriage = new Dictionary<Guid, int>();
        foreach (var group in groupedCarriageAvailabilities)
        {
            var groupList = group.ToList();
            var mask = new BitArray(groupList[0].OccupiedSeats);
            for (int i = 1; i < groupList.Count; i++)
            {
                mask = mask.And(groupList[i].OccupiedSeats);
            }

            availableSeatsPerCarriage[group.Key] = CountSetBits(mask);
        }

        return availableSeatsPerCarriage;
    }

    public async Task<List<int>> GetAvailableSeatsForCarriageAsync(InfoRouteSegmentSearchPerCarriageDto dto)
    {
        var groupedCarriageAvailabilities = await GetAllCarriageAvailabilitiesForRouteAsync(
            MapInfoRouteSegment(dto));
        if (groupedCarriageAvailabilities == null)
        {
            throw new Exception("Carriage availabilities not found");
        }
        
        var targetGroup = groupedCarriageAvailabilities
            .FirstOrDefault(g => g.Key == dto.CarriageTemplateId);
        if(targetGroup == null)
        {
            throw new ArgumentException("Carriage template not found");
        }

        var mask = targetGroup.First().OccupiedSeats;
        foreach (var carriageAvailability in targetGroup)
        {
            mask = mask.And(carriageAvailability.OccupiedSeats);
        }
        
        var availableSeats = new List<int>();
        for(int i = 0; i < mask.Count; i++)
        {
            if (mask[i])
            {
                availableSeats.Add(i + 1);
            }
        }

        return availableSeats;
    }

    public Task<bool> IsSeatAvailable(Guid concreteRouteId, int startSegmentNumber, int endSegmentNumber, int carriageNumber,
        int seatNumber)
    {
        throw new NotImplementedException();
    }
}