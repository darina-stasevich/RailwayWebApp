using RailwayApp.Domain.Entities;

namespace RailwayApp.Domain.Interfaces.IRepositories;

public interface IStationRepository
{
    Task CreateAsync(Station station);
    Task<Station?> GetByNameAsync(string name);
}