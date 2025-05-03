using RailwayApp.Domain.Entities;

namespace RailwayApp.Domain.Interfaces.IRepositories;

public interface ITrainTypeRepository
{
    Task DeleteAllAsync();
    Task<Guid> CreateAsync(TrainType trainType);
    Task<TrainType?> GetByIdAsync(Guid id);
}