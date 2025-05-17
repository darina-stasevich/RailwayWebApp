using RailwayApp.Domain;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces.IRepositories;
using RailwayApp.Domain.Interfaces.IServices.AdminServices;

namespace RailwayApp.Application.Services.AdminServices;

public class AdminCarriageTemplateService(
    ICarriageTemplateRepository carriageTemplateRepository,
    ITrainTypeRepository trainTypeRepository) : IAdminService<CarriageTemplate, Guid>, IAdminCarriageTemplateService
{
    public async Task<IEnumerable<CarriageTemplate>> GetAllItems()
    {
        return await carriageTemplateRepository.GetAllAsync();
    }

    private async Task ValidateCarriageTemplateData(CarriageTemplate item)
    {
        var trainType = await trainTypeRepository.GetByIdAsync(item.TrainTypeId);
        if (trainType == null)
            throw new AdminResourceNotFoundException(nameof(TrainType), item.TrainTypeId);

        if (item.CarriageNumber <= 1)
            throw new AdminValidationException("CarriageNumber must be a positive integer.");

        if (string.IsNullOrWhiteSpace(item.LayoutIdentifier))
            throw new AdminValidationException("LayoutIdentifier cannot be empty.");
        
        if (item.TotalSeats <= 0)
            throw new AdminValidationException("TotalSeats must be a positive integer.");
        
        if (item.PriceMultiplier <= 0)
            throw new AdminValidationException("PriceMultiplier cannot be negative.");
    }

    public async Task<Guid> CreateItem(CarriageTemplate item)
    {
        await ValidateCarriageTemplateData(item);
        
        return await carriageTemplateRepository.AddAsync(item);
    }

    public async Task<bool> UpdateItem(Guid id, CarriageTemplate itemToUpdate)
    {
        if (id != itemToUpdate.Id)
        {
            itemToUpdate.Id = id;
        }

        var existingTemplate = await carriageTemplateRepository.GetByIdAsync(id);
        if (existingTemplate == null)
        {
            throw new AdminResourceNotFoundException(nameof(CarriageTemplate), id);
        }
        
        await ValidateCarriageTemplateData(itemToUpdate);
        bool success = await carriageTemplateRepository.UpdateAsync(itemToUpdate);
        
        return success;
    }

    public async Task<Guid> DeleteItem(Guid id)
    {
        var existingSegment = await carriageTemplateRepository.GetByIdAsync(id);
        if (existingSegment == null)
        {
            throw new AdminResourceNotFoundException(nameof(CarriageTemplate), id);
        }
        
        await carriageTemplateRepository.DeleteAsync(id);
        return id;
    }
}