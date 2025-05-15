using Microsoft.Extensions.Options;
using MongoDB.Driver;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces;
using RailwayApp.Domain.Interfaces.IRepositories;

namespace RailwayApp.Infrastructure.Repositories;

public class MongoDbConcreteRouteSegmentRepository(IMongoClient client, IOptions<MongoDbSettings> settings)
    : MongoDbGenericRepository<ConcreteRouteSegment, Guid>(client, settings, "ConcreteRouteSegments"),
        IConcreteRouteSegmentRepository
{
    public async Task<ConcreteRouteSegment?> GetConcreteSegmentByAbstractSegmentIdAsync(Guid abstractRouteSegmentId,
        DateTime departureDate)
    {
        return await _collection.Find(s =>
                s.AbstractSegmentId == abstractRouteSegmentId && s.ConcreteDepartureDate.Date == departureDate.Date)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<ConcreteRouteSegment>> GetConcreteSegmentsByAbstractSegmentIdAsync(
        Guid abstractSegmentId)
    {
        return await _collection.Find(s => s.AbstractSegmentId == abstractSegmentId).ToListAsync();
    }

    public async Task<IEnumerable<ConcreteRouteSegment>> GetConcreteSegmentsByConcreteRouteIdAsync(Guid concreteRouteId,
        IClientSessionHandle? session = null)
    {
        if (session == null)
            return await _collection.Find(s => s.ConcreteRouteId == concreteRouteId).ToListAsync();
        return await _collection.Find(session, s => s.ConcreteRouteId == concreteRouteId).ToListAsync();
    }

    public async Task<IEnumerable<ConcreteRouteSegment>> GetConcreteSegmentsByFromStationAsync(Guid fromStationId)
    {
        return await _collection.Find(s => s.FromStationId == fromStationId).ToListAsync();
    }

    public async Task<IEnumerable<ConcreteRouteSegment>> GetConcreteSegmentsByToStationAsync(Guid toStationId)
    {
        return await _collection.Find(s => s.ToStationId == toStationId).ToListAsync();
    }
}