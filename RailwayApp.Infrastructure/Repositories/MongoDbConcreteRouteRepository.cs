using Microsoft.Extensions.Options;
using MongoDB.Driver;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces;
using RailwayApp.Domain.Interfaces.IRepositories;

namespace RailwayApp.Infrastructure.Repositories;

public class MongoDbConcreteRouteRepository : IConcreteRouteRepository
{
    private readonly IMongoCollection<ConcreteRoute> _collection;
    
    public MongoDbConcreteRouteRepository(IMongoClient client, IOptions<MongoDbSettings> settings)
    {
        var database = client.GetDatabase(settings.Value.DatabaseName);
        _collection = database.GetCollection<ConcreteRoute>("ConcreteRoutes");
    }
    public async Task DeleteAllAsync()
    {
        var deleteResult = await _collection.DeleteManyAsync(FilterDefinition<ConcreteRoute>.Empty);
    }
    
    public async Task<Guid> CreateAsync(ConcreteRoute route)
    {
        await _collection.InsertOneAsync(route);
        return route.Id;
    }

    public async Task<ConcreteRoute?> GetByIdAsync(Guid id)
    {
        return (await _collection.FindAsync(r => r.Id == id)).FirstOrDefault();
    }
}