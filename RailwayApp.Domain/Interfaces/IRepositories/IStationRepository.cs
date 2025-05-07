using RailwayApp.Domain.Entities;

namespace RailwayApp.Domain.Interfaces.IRepositories;

public interface IStationRepository : IGlobalRepository<Station, Guid>
{
    Task<Station?> GetByNameAsync(string name);
    Task<List<Station>> GetByIdsAsync(List<Guid> ids);
}