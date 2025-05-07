using Microsoft.Extensions.Options;
using MongoDB.Driver;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces;
using RailwayApp.Domain.Interfaces.IRepositories;

namespace RailwayApp.Infrastructure.Repositories;

public class MongoDbConcreteRouteSegmentRepository(IMongoClient client, IOptions<MongoDbSettings> settings)
    : MongoDbGlobalRepository<ConcreteRouteSegment, Guid>(client, settings, "ConcreteRouteSegments"), IConcreteRouteSegmentRepository
{
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