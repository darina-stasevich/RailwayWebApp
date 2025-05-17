using RailwayApp.Domain;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces.IRepositories;
using RailwayApp.Domain.Interfaces.IServices.AdminServices;

namespace RailwayApp.Application.Services.AdminServices;

public class AdminAbstractRouteSegmentService(
    IAbstractRouteSegmentRepository abstractRouteSegmentRepository,
    IAbstractRouteRepository abstractRouteRepository,
    IStationRepository stationRepository) : IAdminService<AbstractRouteSegment, Guid>, IAdminAbstractRouteSegmentService
{
    public async Task<IEnumerable<AbstractRouteSegment>> GetAllItems()
    {
        return await abstractRouteSegmentRepository.GetAllAsync();
    }

    private async Task ValidateAbstractRouteSegmentData(AbstractRouteSegment item, bool isUpdate = false, Guid? existingItemId = null)
    {
        var abstractRoute = await abstractRouteRepository.GetByIdAsync(item.AbstractRouteId);
        if (abstractRoute == null)
            throw new AdminResourceNotFoundException(nameof(AbstractRoute), item.AbstractRouteId);

        var fromStation = await stationRepository.GetByIdAsync(item.FromStationId);
        if (fromStation == null)
            throw new AdminResourceNotFoundException("From " + nameof(Station), item.FromStationId);

        var toStation = await stationRepository.GetByIdAsync(item.ToStationId);
        if (toStation == null)
            throw new AdminResourceNotFoundException("To " + nameof(Station), item.ToStationId);
        
        if (item.FromStationId == item.ToStationId)
            throw new AdminValidationException("FromStationId cannot be the same as ToStationId.");

        var segmentsForRoute = await abstractRouteSegmentRepository.GetAbstractSegmentsByRouteIdAsync(item.AbstractRouteId);
        
        var conflictingSegment = segmentsForRoute.FirstOrDefault(s => s.SegmentNumber == item.SegmentNumber && (!isUpdate || (isUpdate && s.Id != existingItemId)));
        if (conflictingSegment != null)
        {
            throw new AdminDataConflictException(
                $"An {nameof(AbstractRouteSegment)} with SegmentNumber '{item.SegmentNumber}' already exists for AbstractRoute ID '{item.AbstractRouteId}'.");
        }

        if (item.SegmentNumber <= 1)
            throw new AdminValidationException("SegmentNumber must be a positive integer.");

        if (item.SegmentCost < 1)
            throw new AdminValidationException("SegmentCost cannot be negative.");
            
        if (item.FromTime > item.ToTime)
            throw new AdminValidationException("duration must be a positive time span.");
    }

    public async Task<Guid> CreateItem(AbstractRouteSegment item)
    {
        await ValidateAbstractRouteSegmentData(item);
        return await abstractRouteSegmentRepository.AddAsync(item);
    }

    public async Task<bool> UpdateItem(Guid id, AbstractRouteSegment itemToUpdate)
    {
        if (id != itemToUpdate.Id)
        {
            itemToUpdate.Id = id;
        }

        var existingSegment = await abstractRouteSegmentRepository.GetByIdAsync(id);
        if (existingSegment == null)
        {
            throw new AdminResourceNotFoundException(nameof(AbstractRouteSegment), id);
        }

        await ValidateAbstractRouteSegmentData(itemToUpdate, isUpdate: true, existingItemId: id);

        bool success = await abstractRouteSegmentRepository.UpdateAsync(itemToUpdate);
        return success;
    }

    public async Task<Guid> DeleteItem(Guid id)
    {
        var existingSegment = await abstractRouteSegmentRepository.GetByIdAsync(id);
        if (existingSegment == null)
        {
            throw new AdminResourceNotFoundException(nameof(AbstractRouteSegment), id);
        }

        await abstractRouteSegmentRepository.DeleteAsync(id);
        return id;
    }
}