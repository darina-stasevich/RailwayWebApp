using Microsoft.Extensions.Options;
using MongoDB.Driver;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces;
using RailwayApp.Domain.Interfaces.IRepositories;

namespace RailwayApp.Infrastructure.Repositories;

public class MongoDbAbstractRouteRepository : IAbstractRouteRepository
{
    private readonly IMongoCollection<AbstractRoute> _collection;
    
    public MongoDbAbstractRouteRepository(IMongoClient client, IOptions<MongoDbSettings> settings)
    {
        var database = client.GetDatabase(settings.Value.DatabaseName);
        _collection = database.GetCollection<AbstractRoute>("AbstractRoutes");
    }

    public async Task DeleteAllAsync()
    {
        var deleteResult = await _collection.DeleteManyAsync(FilterDefinition<AbstractRoute>.Empty);
    }

    public async Task<Guid> CreateAsync(AbstractRoute route)
    {
        await _collection.InsertOneAsync(route);
        return route.Id;
    }

    public async Task<AbstractRoute?> GetByIdAsync(Guid id)
    {
        return (await _collection.FindAsync(r => r.Id == id)).FirstOrDefault();
    }
}