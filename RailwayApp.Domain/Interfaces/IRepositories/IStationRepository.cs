using RailwayApp.Domain.Entities;

namespace RailwayApp.Domain.Interfaces.IRepositories;

public interface IStationRepository
{
    Task<Guid> CreateAsync(Station station);
    Task<Station?> GetByNameAsync(string name);
    Task<List<Station>> GetAllAsync();
}