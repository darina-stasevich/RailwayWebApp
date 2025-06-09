using RailwayApp.Domain;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces.IRepositories;
using RailwayApp.Domain.Interfaces.IServices.AdminServices;

namespace RailwayApp.Application.Services.AdminServices;

public class AdminTrainTypeService(ITrainTypeRepository trainTypeRepository) :IAdminTrainTypeService
{
    public async Task<IEnumerable<TrainType>> GetAllItems()
    {
        return await trainTypeRepository.GetAllAsync();
    }

    public async Task<TrainType> GetItemByIdAsync(Guid id)
    {
        var trainType = await trainTypeRepository.GetByIdAsync(id);
        if (trainType == null)
            throw new AdminResourceNotFoundException(nameof(TrainType), id);
        return trainType;
    }

    public async Task<Guid> CreateItem(TrainType item)
    {
        return await trainTypeRepository.AddAsync(item);
    }

    public async Task<bool> UpdateItem(Guid id, TrainType itemToUpdate)
    {
        if (id != itemToUpdate.Id)
        {
            itemToUpdate.Id = id;
        }

        var existingTrainType = await trainTypeRepository.GetByIdAsync(id);
        if (existingTrainType == null)
        {
            throw new AdminResourceNotFoundException(nameof(Train), id);
        }
       
        bool success = await trainTypeRepository.UpdateAsync(itemToUpdate);
        return success;
    }

    public async Task<Guid> DeleteItem(Guid id)
    {
        var existingSegment = await trainTypeRepository.GetByIdAsync(id);
        if (existingSegment == null)
        {
            throw new AdminResourceNotFoundException(nameof(TrainType), id);
        }

        await trainTypeRepository.DeleteAsync(id);
        return id;
    }
}