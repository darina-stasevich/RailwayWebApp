using System.Collections;
using MongoDB.Driver.Linq;
using RailwayApp.Application.Models.Dto;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces.IRepositories;
using RailwayApp.Domain.Interfaces.IServices;
using RailwayApp.Domain.Statuses;

namespace RailwayApp.Application.Services;

public class CarriageSeatService(IConcreteRouteRepository concreteRouteRepository,
    IAbstractRouteRepository abstractRouteRepository,
    IAbstractRouteSegmentRepository abstractRouteSegmentRepository,
    IConcreteRouteSegmentRepository concreteRouteSegmentRepository,
    ICarriageAvailabilityRepository carriageAvailabilityRepository,
    ISeatLockRepository seatLockRepository
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

    public async Task<int> GetAvailableSeatsAmountAsync(InfoRouteSegmentSearchDto dto)
    {
        var availabilityGroupedByCarriage = await GetAllCarriageAvailabilitiesForRouteAsync(dto);
        if (availabilityGroupedByCarriage == null)
        {
            throw new Exception("No carriages found found");
        }
        
        int totalAvailableSeats = 0;

        var bookedLockedSeats = await GetBookedSeats(dto);
        var bookedLockedSeatsHashSet = new HashSet<Tuple<Guid, int>>();
        if(bookedLockedSeats.Count() != 0)
        {
            bookedLockedSeatsHashSet = new HashSet<Tuple<Guid, int>>(bookedLockedSeats);
        }
        
        foreach (var group in availabilityGroupedByCarriage)
        {
            var groupList = group.ToList();
            var mask = new BitArray(groupList[0].OccupiedSeats);
            for (int i = 1; i < groupList.Count; i++)
            {
                mask = mask.And(groupList[i].OccupiedSeats);
            }

            if (bookedLockedSeats != null)
            {
                for (int i = 0; i < mask.Count; i++)
                {
                    totalAvailableSeats += bookedLockedSeatsHashSet.Contains(new Tuple<Guid, int>(group.Key, i + 1)) ? 0 : 1;
                }
            }
            else
                totalAvailableSeats += CountSetBits(mask);
        }

        
        
        return totalAvailableSeats;
    }

    public async Task<Dictionary<Guid, int>> GetAvailableSeatCountsPerCarriageAsync(InfoRouteSegmentSearchDto dto)
    {
        var groupedCarriageAvailabilities = await GetAllCarriageAvailabilitiesForRouteAsync(dto);

        if (groupedCarriageAvailabilities == null)
        {
            throw new Exception("Carriage availabilities not found");
        }
        
        var bookedLockedSeats = await GetBookedSeats(dto);
        var bookedLockedSeatsHashSet = new HashSet<Tuple<Guid, int>>();
        if(bookedLockedSeats.Count() != 0)
        {
            bookedLockedSeatsHashSet = new HashSet<Tuple<Guid, int>>(bookedLockedSeats);
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

            int totalAvailableSeats = 0;
            if (bookedLockedSeats.Count() != 0)
            {
                for (int i = 0; i < mask.Count; i++)
                {
                    totalAvailableSeats += bookedLockedSeatsHashSet.Contains(new Tuple<Guid, int>(group.Key, i + 1)) ? 0 : 1;
                }
            }
            else
                totalAvailableSeats += CountSetBits(mask);
            
            availableSeatsPerCarriage[group.Key] = totalAvailableSeats;
        }

        return availableSeatsPerCarriage;
    }

    private async Task<IEnumerable<Tuple<Guid, int> > > GetBookedSeats(InfoRouteSegmentSearchDto dto)
    {
        var seatLocks = await seatLockRepository.GetByRouteIdAsync(dto.ConcreteRouteId);
        var seatLockInfos = seatLocks
            .Where(slis => slis.Status == SeatLockStatus.Active)
            .Where(slis => slis.ExpirationTimeUtc > DateTime.UtcNow)
            .Select(sli => sli.LockedSeatInfos);
        var lockedSeatsInfos = seatLockInfos
            .SelectMany(sli => sli)
            .Where(s => s.ConcreteRouteId == dto.ConcreteRouteId);
        var bookedLockedSeats = lockedSeatsInfos
            .Where(s => s.StartSegmentNumber <= dto.EndSegmentNumber && s.EndSegmentNumber >= dto.StartSegmentNumber)
            .Select(x => new Tuple<Guid, int>(x.CarriageTemplateId, x.SeatNumber));
        
        return bookedLockedSeats;

    }
    
    private async Task<IEnumerable<int> > GetBookedSeatsPerCarriage(InfoRouteSegmentSearchPerCarriageDto dto)
    {
        var seatLocks = await seatLockRepository.GetByRouteIdAsync(dto.ConcreteRouteId);
        var seatLockInfos = seatLocks
            .Where(slis => slis.Status == SeatLockStatus.Active)
            .Where(slis => slis.ExpirationTimeUtc > DateTime.UtcNow)
            .Select(sli => sli.LockedSeatInfos);
        var lockedSeatsInfos = seatLockInfos
            .SelectMany(sli => sli)
            .Where(s => s.ConcreteRouteId == dto.ConcreteRouteId)
            .Where(s => s.CarriageTemplateId == dto.CarriageTemplateId);
        var bookedLockedSeats = lockedSeatsInfos
            .Where(s => s.StartSegmentNumber <= dto.EndSegmentNumber && s.EndSegmentNumber >= dto.StartSegmentNumber)
            .Select(x => x.SeatNumber);
        
        return bookedLockedSeats;

    }
    public async Task<IEnumerable<int>> GetAvailableSeatsForCarriageAsync(InfoRouteSegmentSearchPerCarriageDto dto)
    {
        // 1. get grouped carriage availabilities
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

        // 2. calculate mask of seats which are not paid during the given segment range
        var mask = targetGroup.First().OccupiedSeats;
        foreach (var carriageAvailability in targetGroup)
        {
            mask = mask.And(carriageAvailability.OccupiedSeats);
        }
        
        // 3. get list of not paid seats
        var availableSeats = new List<int>();
        for(int i = 0; i < mask.Count; i++)
        {
            if (mask[i])
            {
                availableSeats.Add(i + 1);
            }
        }
        
        // 4. get active seatLocks for given route 
        
        var bookedLockedSeats = await GetBookedSeatsPerCarriage(dto);
        var bookedLockedSeatsHashSet = new HashSet<int>(bookedLockedSeats);
        var fullAvailableSeats = availableSeats.Select(x => x).Where(x => !bookedLockedSeatsHashSet.Contains(x));
        
        return fullAvailableSeats;

    }

    private InfoRouteSegmentSearchPerCarriageDto MapInfoRouteSegmentSearchPerCarriageDto(InfoSeatSearchDto dto)
    {
        return new InfoRouteSegmentSearchPerCarriageDto
        {
            CarriageTemplateId = dto.CarriageTemplateId,
            ConcreteRouteId = dto.ConcreteRouteId,
            EndSegmentNumber = dto.EndSegmentNumber,
            StartSegmentNumber = dto.StartSegmentNumber
        };
    }
     
    public async Task<bool> IsSeatAvailable(InfoSeatSearchDto dto)
    {
        var seatSearchDto = MapInfoRouteSegmentSearchPerCarriageDto(dto);
        var notPayedSeats = await GetAvailableSeatsForCarriageAsync(seatSearchDto);
        return notPayedSeats.Contains(dto.SeatNumber);
    }
}