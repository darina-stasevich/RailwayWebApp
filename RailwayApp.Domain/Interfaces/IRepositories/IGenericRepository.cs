using MongoDB.Driver;

namespace RailwayApp.Domain.Interfaces.IRepositories;

public interface IGenericRepository<TEntity, TId> where TEntity : class, IEntity<TId>
{
    Task<TEntity?> GetByIdAsync(TId id, IClientSessionHandle? session = null);
    Task<IEnumerable<TEntity>> GetAllAsync();
    Task<TId> AddAsync(TEntity entity);
    Task AddRangeAsync(IEnumerable<TEntity> entities);
    Task DeleteAsync(TId id);
    Task<bool> ExistsAsync(TId id);
    Task DeleteAllAsync();
}