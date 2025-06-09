using RailwayApp.Domain;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces.IRepositories;
using RailwayApp.Domain.Interfaces.IServices.AdminServices;

namespace RailwayApp.Application.Services.AdminServices;

public class AdminTrainService(ITrainRepository trainRepository, ITrainTypeRepository trainTypeRepository)
    : IAdminService<Train, string>, IAdminTrainService
{
    public async Task<IEnumerable<Train>> GetAllItems()
    {
        return await trainRepository.GetAllAsync();
    }

    public async Task<Train> GetItemByIdAsync(string id)
    {
        var train = await trainRepository.GetByIdAsync(id);
        if (train == null)
            throw new AdminResourceNotFoundException(nameof(Train), id);
        return train;
    }

    private async Task ValidateTrainData(Train item)
    {
        var trainType = await trainTypeRepository.GetByIdAsync(item.TrainTypeId);
        if (trainType == null)
            throw new AdminResourceNotFoundException(nameof(TrainType), item.TrainTypeId);
    }

    public async Task<string> CreateItem(Train item)
    {
        await ValidateTrainData(item);
        
        var existingTrain = await trainRepository.GetByIdAsync(item.Id);
        if (existingTrain != null)
             throw new AdminDataConflictException($"Train with Number (ID) '{item.Id}' already exists.");

        return await trainRepository.AddAsync(item);
    }
    public async Task<bool> UpdateItem(string id, Train itemToUpdate)
    {
        if (id != itemToUpdate.Id)
        {
            itemToUpdate.Id = id;
        }

        var existingTrain = await trainRepository.GetByIdAsync(id);
        if (existingTrain == null)
        {
            throw new AdminResourceNotFoundException(nameof(Train), id);
        }
       
        bool success = await trainRepository.UpdateAsync(itemToUpdate);
        return success;
    }

    public async Task<string> DeleteItem(string id)
    {
        var existingSegment = await trainRepository.GetByIdAsync(id);
        if (existingSegment == null)
        {
            throw new AdminResourceNotFoundException(nameof(Train), id);
        }
        
        await trainRepository.DeleteAsync(id);
        return id;
    }
}