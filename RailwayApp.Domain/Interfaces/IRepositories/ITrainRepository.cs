using RailwayApp.Domain.Entities;

namespace RailwayApp.Domain.Interfaces.IRepositories;

public interface ITrainRepository
{
    Task DeleteAllAsync();
    Task<string> CreateAsync(Train train);
    Task<Train?> GetByIdAsync(string id);
}