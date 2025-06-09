using RailwayApp.Domain;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces.IRepositories;
using RailwayApp.Domain.Interfaces.IServices.AdminServices;

namespace RailwayApp.Application.Services.AdminServices;

public class AdminSeatLockService(
    ISeatLockRepository seatLockRepository,
    IConcreteRouteRepository concreteRouteRepository,
    IConcreteRouteSegmentRepository concreteRouteSegmentRepository) :IAdminSeatLockService
{
    public async Task<IEnumerable<SeatLock>> GetAllItems()
    {
        return await seatLockRepository.GetAllAsync();
    }

    public async Task<SeatLock> GetItemByIdAsync(Guid id)
    {
        var seatLock = await seatLockRepository.GetByIdAsync(id);
        if (seatLock == null)
            throw new AdminResourceNotFoundException(nameof(SeatLock), id);
        return seatLock;
    }

    private async Task ValidateSeatLockData(SeatLock item, bool isUpdate = false)
    {
        if (item.LockedSeatInfos == null || !item.LockedSeatInfos.Any())
        {
            throw new AdminValidationException("LockedSeatInfos cannot be null or empty.");
        }

        foreach (var seatInfo in item.LockedSeatInfos)
        {
            var concreteRoute = await concreteRouteRepository.GetByIdAsync(seatInfo.ConcreteRouteId);
            if (concreteRoute == null)
                throw new AdminResourceNotFoundException(nameof(ConcreteRoute), seatInfo.ConcreteRouteId);

            var concreteSegments = (await concreteRouteSegmentRepository.GetConcreteSegmentsByConcreteRouteIdAsync(concreteRoute.Id)).ToList();
            if (!concreteSegments.Any())
            {
                 throw new AdminValidationException($"No concrete segments found for ConcreteRoute ID '{concreteRoute.Id}'. Cannot lock seats.");
            }
            
            var minSegmentNumber = concreteSegments.Min(s => s.SegmentNumber);
            var maxSegmentNumber = concreteSegments.Max(s => s.SegmentNumber);

            if (seatInfo.StartSegmentNumber < minSegmentNumber || seatInfo.StartSegmentNumber > maxSegmentNumber)
                throw new AdminValidationException(
                    $"StartSegmentNumber '{seatInfo.StartSegmentNumber}' is out of range ({minSegmentNumber}-{maxSegmentNumber}) for ConcreteRoute ID '{concreteRoute.Id}'.");

            if (seatInfo.EndSegmentNumber < minSegmentNumber || seatInfo.EndSegmentNumber > maxSegmentNumber)
                throw new AdminValidationException(
                    $"EndSegmentNumber '{seatInfo.EndSegmentNumber}' is out of range ({minSegmentNumber}-{maxSegmentNumber}) for ConcreteRoute ID '{concreteRoute.Id}'.");
            
            if (seatInfo.StartSegmentNumber > seatInfo.EndSegmentNumber)
                throw new AdminValidationException(
                    $"StartSegmentNumber ({seatInfo.StartSegmentNumber}) cannot be greater than EndSegmentNumber ({seatInfo.EndSegmentNumber}).");
        }

        if (isUpdate)
        {
            if (item.ExpirationTimeUtc <= item.CreatedAtTimeUtc)
            {
                throw new AdminValidationException("ExpirationTimeUtc must be after CreatedAtTimeUtc.");
            }
            if (item.ExpirationTimeUtc <= DateTime.UtcNow)
            {
                throw new AdminValidationException("ExpirationTimeUtc for an update cannot be in the past.");
            }
        }
    }

    public async Task<Guid> CreateItem(SeatLock item)
    {
        item.CreatedAtTimeUtc = DateTime.UtcNow;
        item.ExpirationTimeUtc = item.CreatedAtTimeUtc.AddMinutes(20);

        await ValidateSeatLockData(item, isUpdate: false);
        
        if (item.Id != Guid.Empty && await seatLockRepository.ExistsAsync(item.Id))
        {
             throw new AdminDataConflictException($"A {nameof(SeatLock)} with ID '{item.Id}' already exists.");
        }

        return await seatLockRepository.AddAsync(item);
    }

    public async Task<bool> UpdateItem(Guid id, SeatLock itemToUpdate)
    {
        if (id != itemToUpdate.Id)
        {
            itemToUpdate.Id = id;
        }

        var existingLock = await seatLockRepository.GetByIdAsync(id);
        if (existingLock == null)
        {
            throw new AdminResourceNotFoundException(nameof(SeatLock), id);
        }

        await ValidateSeatLockData(itemToUpdate, true);

        bool success = await seatLockRepository.UpdateAsync(itemToUpdate);
        return success;
    }


    public async Task<Guid> DeleteItem(Guid id)
    {
        var existingSegment = await seatLockRepository.GetByIdAsync(id);
        if (existingSegment == null)
        {
            throw new AdminResourceNotFoundException(nameof(SeatLock), id);
        }
        
        await seatLockRepository.DeleteAsync(id);
        return id;
    }
}