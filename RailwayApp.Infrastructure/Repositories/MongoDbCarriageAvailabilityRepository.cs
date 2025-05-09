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

    public async Task<CarriageAvailability> GetByConcreteSegmentIdAndTemplateIdAsync(Guid segmentId, Guid carriageTemplateId,
        IClientSessionHandle session)
    {
        return await _collection
            .Find(session, x => x.ConcreteRouteSegmentId == segmentId && x.CarriageTemplateId == carriageTemplateId)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> UpdateOccupiedSeats(IEnumerable<CarriageAvailability> carriageAvailabilities, IClientSessionHandle session)
    {
        
        var writeModels = new List<WriteModel<CarriageAvailability>>();
        
        foreach (var availability in carriageAvailabilities)
        {
            var filter = Builders<CarriageAvailability>.Filter.Eq(doc => doc.Id, availability.Id);
            var update = Builders<CarriageAvailability>.Update
                .Set(doc => doc.OccupiedSeats, availability.OccupiedSeats);

            writeModels.Add(new UpdateOneModel<CarriageAvailability>(filter, update));
        }

        if (!writeModels.Any())
        {
            return true;
        }

        BulkWriteResult result;
        var bulkOptions = new BulkWriteOptions { IsOrdered = false };
        result = await _collection.BulkWriteAsync(session, writeModels, bulkOptions);
        return result.IsAcknowledged && result.ModifiedCount > 0;
        
    }
}