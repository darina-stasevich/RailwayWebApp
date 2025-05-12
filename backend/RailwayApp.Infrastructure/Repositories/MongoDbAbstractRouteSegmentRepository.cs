using Microsoft.Extensions.Options;
using MongoDB.Driver;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces;
using RailwayApp.Domain.Interfaces.IRepositories;

namespace RailwayApp.Infrastructure.Repositories;

public class MongoDbAbstractRouteSegmentRepository(IMongoClient client, IOptions<MongoDbSettings> settings)
    : MongoDbGenericRepository<AbstractRouteSegment, Guid>(client, settings, "AbstractRouteSegments"), IAbstractRouteSegmentRepository
{
    public async Task<IEnumerable<AbstractRouteSegment>> GetAbstractSegmentsByFromStationAsync(Guid fromStationId)
    {
        return await _collection.Find(s => s.FromStationId == fromStationId).ToListAsync();
    }

    public async Task<IEnumerable<AbstractRouteSegment>> GetAbstractSegmentsByToStationAsync(Guid toStationId)
    {
        return await _collection.Find(s => s.ToStationId == toStationId).ToListAsync();
    }

    public async Task<IEnumerable<AbstractRouteSegment>> GetAbstractSegmentsByRouteIdAsync(Guid routeId, IClientSessionHandle? session = null)
    {
        if(session == null) 
            return await _collection.Find(s => s.AbstractRouteId == routeId).ToListAsync();
        else
            return await _collection.Find(session, s => s.AbstractRouteId == routeId).ToListAsync();
    }
}