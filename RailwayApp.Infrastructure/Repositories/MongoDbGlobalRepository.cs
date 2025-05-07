using Microsoft.Extensions.Options;
using MongoDB.Driver;
using RailwayApp.Domain;
using RailwayApp.Domain.Interfaces;
using RailwayApp.Domain.Interfaces.IRepositories;

namespace RailwayApp.Infrastructure.Repositories;

public abstract class MongoDbGlobalRepository<TEntity, TId> : IGlobalRepository<TEntity, TId> where TEntity : class, IEntity<TId>
{
    protected readonly IMongoCollection<TEntity> _collection;
    
    public MongoDbGlobalRepository(IMongoClient client, IOptions<MongoDbSettings> settings, string collectionName)
    {
        if (string.IsNullOrWhiteSpace(collectionName))
            throw new ArgumentNullException(nameof(collectionName), "collection name is whitespace or null");
        
        var database = client.GetDatabase(settings.Value.DatabaseName);
        _collection = database.GetCollection<TEntity>(collectionName);
    }
    
    public async Task<TEntity?> GetByIdAsync(TId id)
    {
        var filter = Builders<TEntity>.Filter.Eq("_id", id);
        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<TEntity>> GetAllAsync()
    {
        return (await _collection.FindAsync(FilterDefinition<TEntity>.Empty)).ToList();
    }

    public async Task<TId> AddAsync(TEntity entity)
    {
        await _collection.InsertOneAsync(entity);
        return entity.Id;
    }

    public async Task AddRangeAsync(IEnumerable<TEntity> entities)
    {
        await _collection.InsertManyAsync(entities);
    }

    public async Task<bool> ExistsAsync(TId id)
    {
        var filter = Builders<TEntity>.Filter.Eq("_id", id);
        return await _collection.Find(filter).AnyAsync();
    }

    public async Task DeleteAsync(TId id)
    {
        var filter = Builders<TEntity>.Filter.Eq("_id", id);
        await _collection.DeleteOneAsync(filter);
    }

    public async Task DeleteAllAsync()
    {
        await _collection.DeleteManyAsync(FilterDefinition<TEntity>.Empty);
    }
}