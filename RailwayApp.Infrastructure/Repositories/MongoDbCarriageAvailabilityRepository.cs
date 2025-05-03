using Microsoft.Extensions.Options;
using MongoDB.Driver;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces;
using RailwayApp.Domain.Interfaces.IRepositories;

namespace RailwayApp.Infrastructure.Repositories;

public class MongoDbCarriageAvailabilityRepository : ICarriageAvailabilityRepository
{
    private readonly IMongoCollection<CarriageAvailability> _collection;
    
    public MongoDbCarriageAvailabilityRepository(IMongoClient client, IOptions<MongoDbSettings> settings)
    {
        var database = client.GetDatabase(settings.Value.DatabaseName);
        _collection = database.GetCollection<CarriageAvailability>("CarriageAvailabilities");
    }
    public async Task DeleteAllAsync()
    {
        var deleteResult = await _collection.DeleteManyAsync(FilterDefinition<CarriageAvailability>.Empty);
    }

    public async Task<Guid> CreateAsync(CarriageAvailability carriageAvailability)
    {
        await _collection.InsertOneAsync(carriageAvailability);
        return carriageAvailability.Id;
    }

    public async Task<CarriageAvailability?> GetByIdAsync(Guid id)
    {
        return (await _collection.FindAsync(x => x.Id == id)).FirstOrDefault();
    }

    public Task<List<CarriageAvailability>> GetByConcreteSegmentIdAsync(Guid concreteSegmentId)
    {
        return _collection.Find(x => x.ConcreteRouteSegmentId == concreteSegmentId).ToListAsync();
    }
}