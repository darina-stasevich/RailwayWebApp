using RailwayApp.Domain.Entities;

namespace RailwayApp.Domain.Interfaces.IRepositories;

public interface IStationRepository
{
    Task DeleteAllAsync();
    Task<Guid> CreateAsync(Station station);
    Task<Station?> GetByNameAsync(string name);
    Task<Station?> GetByIdAsync(Guid id);
    Task<List<Station>> GetAllAsync();
}