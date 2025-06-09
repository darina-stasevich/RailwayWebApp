using RailwayApp.Domain;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces.IRepositories;
using RailwayApp.Domain.Interfaces.IServices.AdminServices;

namespace RailwayApp.Application.Services.AdminServices;

public class AdminStationService(IStationRepository stationRepository) : IAdminService<Station, Guid>, IAdminStationService
{
    public async Task<IEnumerable<Station>> GetAllItems()
    {
        return await stationRepository.GetAllAsync();
    }

    public async Task<Station> GetItemByIdAsync(Guid id)
    {
        var station = await stationRepository.GetByIdAsync(id);
        if (station == null)
            throw new AdminResourceNotFoundException(nameof(Station), id);
        return station;
    }

    private async Task ValidateStationData(Station item, bool isUpdate = false, Guid? existingItemId = null)
    {
        if (string.IsNullOrWhiteSpace(item.Name))
            throw new AdminValidationException("Station Name cannot be empty.");
        var stationWithName = await stationRepository.GetByNameAsync(item.Name);
        if (stationWithName != null && (!isUpdate || (isUpdate && stationWithName.Id != existingItemId)))
        {
            throw new AdminDataConflictException(
                $"A {nameof(Station)} with Name '{item.Name}' already exists (ID: {stationWithName.Id}).");
        }
    }

    public async Task<Guid> CreateItem(Station item)
    {
        await ValidateStationData(item);
        return await stationRepository.AddAsync(item);
    }
    
    public async Task<bool> UpdateItem(Guid id, Station itemToUpdate)
    {
        if (id != itemToUpdate.Id)
        {
            itemToUpdate.Id = id;
        }

        var existingStation = await stationRepository.GetByIdAsync(id);
        if (existingStation == null)
        {
            throw new AdminResourceNotFoundException(nameof(Station), id);
        }

        await ValidateStationData(itemToUpdate, true, id);

        bool success = await stationRepository.UpdateAsync(itemToUpdate);
        return success;
    }

    public async Task<Guid> DeleteItem(Guid id)
    {
        var existingSegment = await stationRepository.GetByIdAsync(id);
        if (existingSegment == null)
        {
            throw new AdminResourceNotFoundException(nameof(Station), id);
        }
        
        await stationRepository.DeleteAsync(id);
        return id;
    }
}