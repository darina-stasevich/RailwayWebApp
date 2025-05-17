using RailwayApp.Domain;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces.IRepositories;
using RailwayApp.Domain.Interfaces.IServices.AdminServices;

namespace RailwayApp.Application.Services.AdminServices;

public class AdminCarriageAvailabilityService(
    ICarriageAvailabilityRepository carriageAvailabilityRepository,
    IConcreteRouteSegmentRepository concreteRouteSegmentRepository,
    ICarriageTemplateRepository carriageTemplateRepository)
    : IAdminService<CarriageAvailability, Guid>, IAdminCarriageAvailabilityService
{
    public async Task<IEnumerable<CarriageAvailability>> GetAllItems()
    {
        return await carriageAvailabilityRepository.GetAllAsync();
    }

   private async Task ValidateCarriageAvailabilityData(CarriageAvailability item, bool isUpdate = false, Guid? existingItemId = null)
    {
        var concreteSegment = await concreteRouteSegmentRepository.GetByIdAsync(item.ConcreteRouteSegmentId);
        if (concreteSegment == null)
            throw new AdminResourceNotFoundException(nameof(ConcreteRouteSegment), item.ConcreteRouteSegmentId);

        var carriageTemplate = await carriageTemplateRepository.GetByIdAsync(item.CarriageTemplateId); 
        if (carriageTemplate == null)
            throw new AdminResourceNotFoundException(nameof(CarriageTemplate), item.CarriageTemplateId);

        var existingAvailabilitiesForSegment = await carriageAvailabilityRepository.GetByConcreteSegmentIdAsync(item.ConcreteRouteSegmentId);
        
        var conflictingAvailability = existingAvailabilitiesForSegment.FirstOrDefault(ca => 
            ca.CarriageTemplateId == item.CarriageTemplateId &&
            (!isUpdate || (isUpdate && ca.Id != existingItemId))); 

        if (conflictingAvailability != null)
        {
            throw new AdminDataConflictException(
                $"A {nameof(CarriageAvailability)} for {nameof(CarriageTemplate)} ID '{item.CarriageTemplateId}' (CarriageNumber: {carriageTemplate.CarriageNumber}) " +
                $"already exists for {nameof(ConcreteRouteSegment)} ID '{item.ConcreteRouteSegmentId}'.");
        }
    }

    public async Task<Guid> CreateItem(CarriageAvailability item)
    {
        await ValidateCarriageAvailabilityData(item);
        return await carriageAvailabilityRepository.AddAsync(item);
    }

    public async Task<bool> UpdateItem(Guid id, CarriageAvailability itemToUpdate)
    {
        if (id != itemToUpdate.Id)
        {
            itemToUpdate.Id = id;
        }

        var existingAvailability = await carriageAvailabilityRepository.GetByIdAsync(id);
        if (existingAvailability == null)
        {
            throw new AdminResourceNotFoundException(nameof(CarriageAvailability), id);
        }

        var newCarriageTemplate = await carriageTemplateRepository.GetByIdAsync(itemToUpdate.CarriageTemplateId);
        if (newCarriageTemplate == null)
            throw new AdminResourceNotFoundException(nameof(CarriageTemplate), itemToUpdate.CarriageTemplateId);
        
        await ValidateCarriageAvailabilityData(itemToUpdate, true, id);

        bool success = await carriageAvailabilityRepository.UpdateAsync(itemToUpdate);
        
        return success;
    }


    public async Task<Guid> DeleteItem(Guid id)
    {
        var existingSegment = await carriageAvailabilityRepository.GetByIdAsync(id);
        if (existingSegment == null)
        {
            throw new AdminResourceNotFoundException(nameof(CarriageAvailability), id);
        }

        await carriageAvailabilityRepository.DeleteAsync(id);
        return id;
    }
}