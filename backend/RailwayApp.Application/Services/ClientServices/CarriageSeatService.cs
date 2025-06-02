using System.Collections;
using MongoDB.Driver;
using RailwayApp.Application.Models.Dto;
using RailwayApp.Domain;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces.IRepositories;
using RailwayApp.Domain.Interfaces.IServices;
using RailwayApp.Domain.Statuses;

namespace RailwayApp.Application.Services;

public class CarriageSeatService(
    IUnitOfWork unitOfWork
) : ICarriageSeatService
{
    public async Task<int> GetAvailableSeatsAmountAsync(InfoRouteSegmentSearchDto dto)
    {
        var availabilityGroupedByCarriage = await GetAllCarriageAvailabilitiesForRouteAsync(dto);
        if (availabilityGroupedByCarriage == null)
            throw new CarriageSeatServiceCarriageAvailabilityNotFoundException(dto.ConcreteRouteId,
                dto.StartSegmentNumber, dto.EndSegmentNumber);

        var totalAvailableSeats = 0;

        var bookedLockedSeats = await GetBookedSeats(dto);
        var bookedLockedSeatsHashSet = new HashSet<Tuple<Guid, int>>();
        if (bookedLockedSeats.Count() != 0) bookedLockedSeatsHashSet = new HashSet<Tuple<Guid, int>>(bookedLockedSeats);

        foreach (var group in availabilityGroupedByCarriage)
        {
            var groupList = group.ToList();
            var mask = new BitArray(groupList[0].OccupiedSeats);
            for (var i = 1; i < groupList.Count; i++) mask = mask.And(groupList[i].OccupiedSeats);

            if (bookedLockedSeats != null)
                for (var i = 0; i < mask.Count; i++)
                    totalAvailableSeats += bookedLockedSeatsHashSet.Contains(new Tuple<Guid, int>(group.Key, i + 1))
                        ? 0
                        : 1;
            else
                totalAvailableSeats += CountSetBits(mask);
        }


        return totalAvailableSeats;
    }

    public async Task<Dictionary<Guid, int>> GetAvailableSeatCountsPerCarriageAsync(InfoRouteSegmentSearchDto dto)
    {
        var groupedCarriageAvailabilities = await GetAllCarriageAvailabilitiesForRouteAsync(dto);

        if (groupedCarriageAvailabilities == null)
            throw new CarriageSeatServiceCarriageAvailabilityNotFoundException(dto.ConcreteRouteId,
                dto.StartSegmentNumber, dto.EndSegmentNumber);

        var bookedLockedSeats = await GetBookedSeats(dto);
        var bookedLockedSeatsHashSet = new HashSet<Tuple<Guid, int>>();
        if (bookedLockedSeats.Count() != 0) bookedLockedSeatsHashSet = new HashSet<Tuple<Guid, int>>(bookedLockedSeats);

        var availableSeatsPerCarriage = new Dictionary<Guid, int>();
        foreach (var group in groupedCarriageAvailabilities)
        {
            var groupList = group.ToList();
            var mask = new BitArray(groupList[0].OccupiedSeats);
            for (var i = 1; i < groupList.Count; i++) mask = mask.And(groupList[i].OccupiedSeats);

            var totalAvailableSeats = 0;
            if (bookedLockedSeats.Count() != 0)
                for (var i = 0; i < mask.Count; i++)
                    totalAvailableSeats += bookedLockedSeatsHashSet.Contains(new Tuple<Guid, int>(group.Key, i + 1))
                        ? 0
                        : 1;
            else
                totalAvailableSeats += CountSetBits(mask);

            availableSeatsPerCarriage[group.Key] = totalAvailableSeats;
        }

        return availableSeatsPerCarriage;
    }

    public async Task<IEnumerable<int>> GetAvailableSeatsForCarriageAsync(InfoRouteSegmentSearchPerCarriageDto dto,
        IClientSessionHandle? session = null)
    {
        // 1. get grouped carriage availabilities
        var groupedCarriageAvailabilities = await GetAllCarriageAvailabilitiesForRouteAsync(
            MapInfoRouteSegment(dto), session);
        if (groupedCarriageAvailabilities == null)
            throw new CarriageSeatServiceCarriageAvailabilityNotFoundException(dto.ConcreteRouteId,
                dto.StartSegmentNumber, dto.EndSegmentNumber);

        var targetGroup = groupedCarriageAvailabilities
            .FirstOrDefault(g => g.Key == dto.CarriageTemplateId);
        if (targetGroup == null)
            throw new CarriageTemplateNotFoundException(dto.CarriageTemplateId,
                dto.CarriageTemplateId, dto.StartSegmentNumber, dto.EndSegmentNumber);

        // 2. calculate mask of seats which are not paid during the given segment range
        var mask = targetGroup.First().OccupiedSeats;
        foreach (var carriageAvailability in targetGroup) mask = mask.And(carriageAvailability.OccupiedSeats);

        // 3. get list of not paid seats
        var availableSeats = new List<int>();
        for (var i = 0; i < mask.Count; i++)
            if (mask[i])
                availableSeats.Add(i + 1);

        // 4. get active seatLocks for given route 

        var bookedLockedSeats = await GetBookedSeatsPerCarriage(dto, session);
        var bookedLockedSeatsHashSet = new HashSet<int>(bookedLockedSeats);
        var fullAvailableSeats = availableSeats.Select(x => x).Where(x => !bookedLockedSeatsHashSet.Contains(x));

        return fullAvailableSeats;
    }

    public async Task<bool> IsSeatAvailable(InfoSeatSearchDto dto, IClientSessionHandle? session = null)
    {
        var seatSearchDto = MapInfoRouteSegmentSearchPerCarriageDto(dto);
        var notPayedSeats = await GetAvailableSeatsForCarriageAsync(seatSearchDto, session);
        return notPayedSeats.Contains(dto.SeatNumber);
    }

    private InfoRouteSegmentSearchDto MapInfoRouteSegment(InfoRouteSegmentSearchPerCarriageDto dto)
    {
        return new InfoRouteSegmentSearchDto
        {
            ConcreteRouteId = dto.ConcreteRouteId,
            StartSegmentNumber = dto.StartSegmentNumber,
            EndSegmentNumber = dto.EndSegmentNumber
        };
    }

    private async Task<IEnumerable<IGrouping<Guid, CarriageAvailability>>?> GetAllCarriageAvailabilitiesForRouteAsync(
        InfoRouteSegmentSearchDto dto, IClientSessionHandle? session = null)
    {
        var concreteRouteSegments =
            await unitOfWork.ConcreteRouteSegments.GetConcreteSegmentsByConcreteRouteIdAsync(dto.ConcreteRouteId,
                session);
        var relevantConcreteRouteSegments = concreteRouteSegments.Where(s =>
            s.SegmentNumber >= dto.StartSegmentNumber && s.SegmentNumber <= dto.EndSegmentNumber);

        var allAvailabilities = new List<CarriageAvailability>();
        foreach (var routeSegment in relevantConcreteRouteSegments)
        {
            var carriageAvailabilities =
                await unitOfWork.CarriageAvailabilities.GetByConcreteSegmentIdAsync(routeSegment.Id, session);
            allAvailabilities.AddRange(carriageAvailabilities);
        }

        var availabilityGroupedByCarriageTemplateId = allAvailabilities
            .GroupBy(a => a.CarriageTemplateId);

        return availabilityGroupedByCarriageTemplateId;
    }

    private int CountSetBits(BitArray bitArray)
    {
        var count = 0;
        foreach (bool bit in bitArray)
            if (bit)
                count++;
        return count;
    }

    private async Task<IEnumerable<Tuple<Guid, int>>> GetBookedSeats(InfoRouteSegmentSearchDto dto)
    {
        var seatLocks = await unitOfWork.SeatLocks.GetByRouteIdAsync(dto.ConcreteRouteId);
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

    private async Task<IEnumerable<int>> GetBookedSeatsPerCarriage(InfoRouteSegmentSearchPerCarriageDto dto,
        IClientSessionHandle? session = null)
    {
        var seatLocks = await unitOfWork.SeatLocks.GetByRouteIdAsync(dto.ConcreteRouteId, session);
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
}