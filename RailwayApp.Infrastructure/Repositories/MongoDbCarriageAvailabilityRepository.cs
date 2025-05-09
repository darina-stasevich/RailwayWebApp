using Microsoft.Extensions.Options;
using MongoDB.Driver;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces;
using RailwayApp.Domain.Interfaces.IRepositories;

namespace RailwayApp.Infrastructure.Repositories;

public class MongoDbCarriageAvailabilityRepository(IMongoClient client, IOptions<MongoDbSettings> settings)
    : MongoDbGenericRepository<CarriageAvailability, Guid>(client, settings, "CarriageAvailabilities"), ICarriageAvailabilityRepository
{
    public async Task<IEnumerable<CarriageAvailability>> GetByConcreteSegmentIdAsync(Guid concreteSegmentId)
    {
        return await _collection.Find(x => x.ConcreteRouteSegmentId == concreteSegmentId).ToListAsync();
    }
}