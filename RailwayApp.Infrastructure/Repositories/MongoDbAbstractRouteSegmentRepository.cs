using Microsoft.Extensions.Options;
using MongoDB.Driver;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces;
using RailwayApp.Domain.Interfaces.IRepositories;

namespace RailwayApp.Infrastructure.Repositories;

public class MongoDbAbstractRouteSegmentRepository(IMongoClient client, IOptions<MongoDbSettings> settings)
    : MongoDbGlobalRepository<AbstractRouteSegment, Guid>(client, settings, "AbstractRouteSegments"), IAbstractRouteSegmentRepository
{
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