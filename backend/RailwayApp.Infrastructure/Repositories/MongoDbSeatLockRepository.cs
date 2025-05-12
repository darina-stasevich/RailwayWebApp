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
    public async Task<IEnumerable<SeatLock>> GetByRouteIdAsync(Guid concreteRouteId, IClientSessionHandle? session = null)
    {
        var filter = Builders<SeatLock>.Filter.ElemMatch(
            sl => sl.LockedSeatInfos,
            lsi => lsi.ConcreteRouteId == concreteRouteId);
        if(session == null)
            return await _collection.Find(filter).ToListAsync();
        else
            return await _collection.Find(session, filter).ToListAsync();
    }

    public async Task<IEnumerable<SeatLock>> GetByUserAccountIdAsync(Guid userAccountId)
    {
        return await _collection.Find(x => x.UserAccountId == userAccountId).ToListAsync();
    }

    public async Task<bool> UpdateStatusAsync(Guid seatLockId, SeatLockStatus status, IClientSessionHandle? session = null)
    {
        var filter = Builders<SeatLock>.Filter.Eq(u => u.Id, seatLockId);
        UpdateResult? updateResult = null;
        if (session == null)
        {
            var update = Builders<SeatLock>.Update
                .Set(s => s.Status, status);
            updateResult = await _collection.UpdateOneAsync(filter, update);
        }
        else
        {
            var update = Builders<SeatLock>.Update
                .Set(s => s.Status, status);
            updateResult = await _collection.UpdateOneAsync(session, filter, update);
        }

        return updateResult.IsAcknowledged && updateResult.MatchedCount > 0;
    }

    public async Task<bool> PrepareForProcessingAsync(Guid seatLockId, DateTime newExpirationTime, SeatLockStatus newStatus, SeatLockStatus expectedCurrentStatus, IClientSessionHandle session)
    {
        var filter = Builders<SeatLock>.Filter.And(
            Builders<SeatLock>.Filter.Eq(sl => sl.Id, seatLockId),
            Builders<SeatLock>.Filter.Eq(sl => sl.Status, expectedCurrentStatus)
        );

        var update = Builders<SeatLock>.Update
            .Set(sl => sl.Status, newStatus)
            .Set(sl => sl.ExpirationTimeUtc, newExpirationTime);

        var result = await _collection.UpdateOneAsync(session, filter, update);
        return result.IsAcknowledged && result.ModifiedCount > 0;
    }

}