using Microsoft.Extensions.Options;
using MongoDB.Driver;
using RailwayApp.Domain;
using RailwayApp.Domain.Interfaces;
using RailwayApp.Domain.Interfaces.IRepositories;

namespace RailwayApp.Infrastructure.Repositories;

public abstract class MongoDbGenericRepository<TEntity, TId> : IGenericRepository<TEntity, TId>
    where TEntity : class, IEntity<TId>
{
    protected readonly IMongoCollection<TEntity> _collection;

    public MongoDbGenericRepository(IMongoClient client, IOptions<MongoDbSettings> settings, string collectionName)
    {
        if (string.IsNullOrWhiteSpace(collectionName))
            throw new ArgumentNullException(nameof(collectionName), "collection name is whitespace or null");

        var database = client.GetDatabase(settings.Value.DatabaseName);
        _collection = database.GetCollection<TEntity>(collectionName);
    }

    public async Task<TEntity?> GetByIdAsync(TId id, IClientSessionHandle? session = null)
    {
        var filter = Builders<TEntity>.Filter.Eq("_id", id);

        if (session == null)
            return await _collection.Find(filter).FirstOrDefaultAsync();
        return await _collection.Find(session, filter).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<TEntity>> GetAllAsync()
    {
        return await _collection.Find(FilterDefinition<TEntity>.Empty).ToListAsync();
    }

    public async Task<TId> AddAsync(TEntity entity, IClientSessionHandle? session = null)
    {
        if (session == null)
            await _collection.InsertOneAsync(entity);
        else
            await _collection.InsertOneAsync(session, entity);
        return entity.Id;
    }

    public async Task AddRangeAsync(IEnumerable<TEntity> entities, IClientSessionHandle? session = null)
    {
        if (session == null)
            await _collection.InsertManyAsync(entities);
        else
            await _collection.InsertManyAsync(session, entities);
    }

    public async Task<bool> UpdateAsync(TEntity entity, IClientSessionHandle? session = null)
    {
        var filter = Builders<TEntity>.Filter.Eq(e => e.Id, entity.Id);
        ReplaceOneResult result;

        if (session == null)
            result = await _collection.ReplaceOneAsync(filter, entity);
        else
            result = await _collection.ReplaceOneAsync(session, filter, entity);
            
        return result.IsAcknowledged && result.ModifiedCount > 0;
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