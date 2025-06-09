using RailwayApp.Domain;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces.IRepositories;
using RailwayApp.Domain.Interfaces.IServices.AdminServices;

namespace RailwayApp.Application.Services.AdminServices;

public class AdminConcreteRouteService(
    IConcreteRouteRepository concreteRouteRepository,
    IAbstractRouteRepository abstractRouteRepository) :IAdminConcreteRouteService
{
    public async Task<IEnumerable<ConcreteRoute>> GetAllItems()
    {
        return await concreteRouteRepository.GetAllAsync();
    }

    public async Task<ConcreteRoute> GetItemByIdAsync(Guid id)
    {
        var concreteRoute = await concreteRouteRepository.GetByIdAsync(id);
        if (concreteRoute == null)
            throw new AdminResourceNotFoundException(nameof(concreteRoute), id);
        return concreteRoute;
    }

    private async Task ValidateConcreteRouteData(ConcreteRoute item, bool isUpdate = false, Guid? existingItemId = null)
    {
        var abstractRoute = await abstractRouteRepository.GetByIdAsync(item.AbstractRouteId);
        if (abstractRoute == null)
            throw new AdminResourceNotFoundException(nameof(AbstractRoute), item.AbstractRouteId);

        if (abstractRoute.DepartureTime != item.RouteDepartureDate.TimeOfDay)
        {
            throw new AdminValidationException(
                $"The time part of RouteDepartureDate ({item.RouteDepartureDate.TimeOfDay}) " +
                $"must match the DepartureTime of the associated AbstractRoute ({abstractRoute.DepartureTime}).");
        }
    }

    public async Task<Guid> CreateItem(ConcreteRoute item)
    {
        await ValidateConcreteRouteData(item);
        return await concreteRouteRepository.AddAsync(item);
    }

    public async Task<bool> UpdateItem(Guid id, ConcreteRoute itemToUpdate)
    {
        if (id != itemToUpdate.Id)
        {
            itemToUpdate.Id = id;
        }

        var existingConcreteRoute = await concreteRouteRepository.GetByIdAsync(id);
        if (existingConcreteRoute == null)
        {
            throw new AdminResourceNotFoundException(nameof(ConcreteRoute), id);
        }
        
        await ValidateConcreteRouteData(itemToUpdate, isUpdate: true, existingItemId: id);

        bool success = await concreteRouteRepository.UpdateAsync(itemToUpdate);
        return success;
    }

    public async Task<Guid> DeleteItem(Guid id)
    {
        var existingSegment = await concreteRouteRepository.GetByIdAsync(id);
        if (existingSegment == null)
        {
            throw new AdminResourceNotFoundException(nameof(ConcreteRoute), id);
        }
        await concreteRouteRepository.DeleteAsync(id);
        return id;
    }
}