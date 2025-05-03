using Microsoft.Extensions.Options;
using MongoDB.Driver;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces;
using RailwayApp.Domain.Interfaces.IRepositories;

namespace RailwayApp.Infrastructure.Repositories;

public class MongoDbConcreteRouteSegmentRepository : IConcreteRouteSegmentRepository
{
    private readonly IMongoCollection<ConcreteRouteSegment> _collection;
    
    public MongoDbConcreteRouteSegmentRepository(IMongoClient client, IOptions<MongoDbSettings> settings)
    {
        var database = client.GetDatabase(settings.Value.DatabaseName);
        _collection = database.GetCollection<ConcreteRouteSegment>("ConcreteRouteSegments");
    }
    public async Task DeleteAllAsync()
    {
        var deleteResult = await _collection.DeleteManyAsync(FilterDefinition<ConcreteRouteSegment>.Empty);
    }
    
    public async Task<Guid> CreateAsync(ConcreteRouteSegment segment)
    {
        await _collection.InsertOneAsync(segment);
        return segment.Id;
    }

    public async Task<ConcreteRouteSegment?> GetConcreteSegmentByAbstractSegmentIdAsync(Guid abstractRouteSegmentId, DateTime departureDate)
    {
        var result = await _collection.FindAsync(s =>
            s.AbstractSegmentId == abstractRouteSegmentId && s.ConcreteDepartureDate.Date == departureDate.Date);
        return result.FirstOrDefault();
    }

    public async Task<List<ConcreteRouteSegment>> GetConcreteSegmentsByAbstractSegmentIdAsync(Guid abstractSegmentId)
    {
        return (await _collection.FindAsync(s => s.AbstractSegmentId == abstractSegmentId)).ToList();
    }

    public async Task<List<ConcreteRouteSegment>> GetConcreteSegmentsByConcreteRouteIdAsync(Guid concreteRouteId)
    {
        return (await _collection.FindAsync(s => s.ConcreteRouteId == concreteRouteId)).ToList();
    }
}