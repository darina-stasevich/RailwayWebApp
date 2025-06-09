using RailwayApp.Domain;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces.IRepositories;
using RailwayApp.Domain.Interfaces.IServices.AdminServices;

namespace RailwayApp.Application.Services.AdminServices;

public class AdminAbstractRouteService(
    IAbstractRouteRepository abstractRouteRepository,
    ITrainRepository trainRepository) : IAdminAbstractRouteService
{
    public async Task<IEnumerable<AbstractRoute>> GetAllItems()
    {
        return await abstractRouteRepository.GetAllAsync();
    }

    public async Task<AbstractRoute> GetItemByIdAsync(Guid id)
    {
        var route = await abstractRouteRepository.GetByIdAsync(id);
        if (route == null)
        {
            throw new AdminResourceNotFoundException(nameof(AbstractRoute), id);
        } 
        return route;
    }

    private async Task ValidateAbstractRouteData(AbstractRoute item)
    {
        var train = await trainRepository.GetByIdAsync(item.TrainNumber);
        if (train == null)
            throw new AdminResourceNotFoundException(nameof(Train), item.TrainNumber);

        if (string.IsNullOrWhiteSpace(item.TrainNumber)) 
            throw new AdminValidationException("TrainNumber cannot be empty.");

        if (!item.ActiveDays.Any())
        {
            throw new AdminValidationException("ActiveDays cannot be empty. Use 'ежедневно' or comma-separated day numbers (1-7).");
        }
        
        if (item.TransferCost < 0)
        {
            throw new AdminValidationException("TransferCost cannot be negative.");
        }
    }

    public async Task<Guid> CreateItem(AbstractRoute item)
    {
        await ValidateAbstractRouteData(item);
        return await abstractRouteRepository.AddAsync(item);
    }

    public async Task<bool> UpdateItem(Guid id, AbstractRoute itemToUpdate)
    {
        if (id != itemToUpdate.Id)
        {
            itemToUpdate.Id = id;
        }

        var existingRoute = await abstractRouteRepository.GetByIdAsync(id);
        if (existingRoute == null)
        {
            throw new AdminResourceNotFoundException(nameof(AbstractRoute), id);
        }

        await ValidateAbstractRouteData(itemToUpdate);
       
        bool success = await abstractRouteRepository.UpdateAsync(itemToUpdate);
        return success;
    }
    public async Task<Guid> DeleteItem(Guid id)
    {
        var existingSegment = await abstractRouteRepository.GetByIdAsync(id);
        if (existingSegment == null)
        {
            throw new AdminResourceNotFoundException(nameof(AbstractRoute), id);
        }
        
        await abstractRouteRepository.DeleteAsync(id);
        return id;
    }
}