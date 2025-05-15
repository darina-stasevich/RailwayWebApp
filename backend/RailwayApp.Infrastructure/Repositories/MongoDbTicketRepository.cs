// RailwayApp.Infrastructure/Persistence/Neo4j/Repositories/TicketRepository.cs

using Microsoft.Extensions.Options;
using MongoDB.Driver;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces;
using RailwayApp.Domain.Interfaces.IRepositories;
using RailwayApp.Domain.Statuses;

namespace RailwayApp.Infrastructure.Repositories;

public class MongoDbTicketRepository(IMongoClient client, IOptions<MongoDbSettings> settings)
    : MongoDbGenericRepository<Ticket, Guid>(client, settings, "Tickets"), ITicketRepository
{
    public async Task AddRange(IEnumerable<Ticket> tickets, IClientSessionHandle session)
    {
        await _collection.InsertManyAsync(session, tickets);
    }

    public async Task<IEnumerable<Ticket>> GetByUserAccountIdAsync(Guid userAccountId)
    {
        return await _collection.Find(t => t.UserAccountId == userAccountId).ToListAsync();
    }

    public async Task<bool> UpdateStatusAsync(Guid id, TicketStatus status, IClientSessionHandle session)
    {
        var filter = Builders<Ticket>.Filter.Eq(t => t.Id, id);
        var update = Builders<Ticket>.Update.Set(t => t.Status, status);
        var updateResult = await _collection.UpdateOneAsync(session, filter, update);
        return updateResult.IsAcknowledged && updateResult.ModifiedCount > 0;
    }
}