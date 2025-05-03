using Microsoft.Extensions.Options;
using MongoDB.Driver;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces;
using RailwayApp.Domain.Interfaces.IRepositories;

namespace RailwayApp.Infrastructure.Repositories;

public class MongoDbAbstractRouteSegmentRepository : IAbstractRouteSegmentRepository
{
    private readonly IMongoCollection<AbstractRouteSegment> _collection;
    
    public MongoDbAbstractRouteSegmentRepository(IMongoClient client, IOptions<MongoDbSettings> settings)
    {
        var database = client.GetDatabase(settings.Value.DatabaseName);
        _collection = database.GetCollection<AbstractRouteSegment>("AbstractRouteSegments");
    }
    
    public async Task DeleteAllAsync()
    {
        var deleteResult = await _collection.DeleteManyAsync(FilterDefinition<AbstractRouteSegment>.Empty);
    }
    
    public async Task<Guid> CreateAsync(AbstractRouteSegment segment)
    {
        await _collection.InsertOneAsync(segment);
        return segment.Id;
    }

    public async Task<List<AbstractRouteSegment>> GetAbstractSegmentsByFromStationAsync(Guid fromStationId)
    {
        return (await _collection.FindAsync(s => s.FromStationId == fromStationId)).ToList();
    }

    public async Task<List<AbstractRouteSegment>> GetAbstractSegmentsByToStationAsync(Guid toStationId)
    {
        return (await _collection.FindAsync(s => s.ToStationId == toStationId)).ToList();
    }

    public async Task<List<AbstractRouteSegment>> GetAbstractSegmentsByRouteIdAsync(Guid routeId)
    {
        return (await _collection.FindAsync(s => s.AbstractRouteId == routeId)).ToList();
    }
}