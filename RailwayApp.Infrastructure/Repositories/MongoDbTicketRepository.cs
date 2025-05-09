// RailwayApp.Infrastructure/Persistence/Neo4j/Repositories/TicketRepository.cs

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces;
using RailwayApp.Domain.Interfaces.IRepositories;

namespace RailwayApp.Infrastructure.Repositories;

public class MongoDbTicketRepository(IMongoClient client, IOptions<MongoDbSettings> settings)
    : MongoDbGenericRepository<Ticket, Guid>(client, settings, "Tickets"), ITicketRepository
{
    public async Task<IEnumerable<Ticket>> GetByUserAccountIdAsync(Guid id)
    {
        return await _collection.Find(t => t.Id == id).ToListAsync();
    }
}