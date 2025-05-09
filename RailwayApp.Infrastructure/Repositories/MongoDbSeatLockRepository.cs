using Microsoft.Extensions.Options;
using MongoDB.Driver;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces;
using RailwayApp.Domain.Interfaces.IRepositories;
using RailwayApp.Domain.Statuses;

namespace RailwayApp.Infrastructure.Repositories;

public class MongoDbSeatLockRepository(IMongoClient client, IOptions<MongoDbSettings> settings)
    : MongoDbGenericRepository<SeatLock, Guid>(client, settings, "SeatLocks"), ISeatLockRepository
{
    public async Task<IEnumerable<SeatLock>> GetByRouteIdAsync(Guid concreteRouteId)
    {
        var filter = Builders<SeatLock>.Filter.ElemMatch(
            sl => sl.LockedSeatInfos,
            lsi => lsi.ConcreteRouteId == concreteRouteId);
        
        return await _collection.Find(filter).ToListAsync();
    }

    public async Task<bool> UpdateStatusAsync(Guid seatLockId, SeatLockStatus status)
    {
        var filter = Builders<SeatLock>.Filter.Eq(u => u.Id, seatLockId);
        var update = Builders<SeatLock>.Update
            .Set(s => s.Status, status);
        var updateResult = await _collection.UpdateOneAsync(filter, update);

        return updateResult.IsAcknowledged && updateResult.MatchedCount > 0;
    }

    public async Task<bool> UpdateExpirationTimeAsync(Guid seatLockId, DateTime newExpirationDateUtc)
    {
        var filter = Builders<SeatLock>.Filter.Eq(u => u.Id, seatLockId);
        var update = Builders<SeatLock>.Update
            .Set(s => s.ExpirationTimeUtc, newExpirationDateUtc);
        var updateResult = await _collection.UpdateOneAsync(filter, update);

        return updateResult.IsAcknowledged && updateResult.MatchedCount > 0;
    }
}