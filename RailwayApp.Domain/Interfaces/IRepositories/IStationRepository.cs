using RailwayApp.Domain.Entities;

namespace RailwayApp.Domain.Interfaces.IRepositories;

public interface IStationRepository : IGenericRepository<Station, Guid>
{
    Task<Station?> GetByNameAsync(string name);
    Task<IEnumerable<Station>> GetByIdsAsync(List<Guid> ids);
}