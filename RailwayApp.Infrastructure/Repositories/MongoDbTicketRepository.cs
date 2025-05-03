// RailwayApp.Infrastructure/Persistence/Neo4j/Repositories/TicketRepository.cs

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces;
using RailwayApp.Domain.Interfaces.IRepositories;

namespace RailwayApp.Infrastructure.Repositories;

public class MongoDbTicketRepository : ITicketRepository
{
    private readonly IMongoCollection<Ticket> _collection;
    public MongoDbTicketRepository(IMongoClient client, IOptions<MongoDbSettings> settings)
    {
        var database = client.GetDatabase(settings.Value.DatabaseName);
        _collection = database.GetCollection<Ticket>("Tickets");
        
        var indexKeys = Builders<Ticket>.IndexKeys.Ascending(t => t.UserAccountEmail);
        _collection.Indexes.CreateOne(new CreateIndexModel<Ticket>(indexKeys));
    }

    public async Task DeleteAllAsync()
    {
        var deleteResult = await _collection.DeleteManyAsync(FilterDefinition<Ticket>.Empty);
    }
    
    public async Task<IEnumerable<Ticket>> GetByUserEmailAsync(string email)
    {
        return await _collection.Find(t => t.UserAccountEmail == email).ToListAsync();
    }

    public async Task<Ticket> GetByIdAsync(Guid id)
    {
        return await _collection.Find(t => t.Id == id).FirstOrDefaultAsync();
    }

    public async Task<Guid> CreateAsync(Ticket ticket)
    {
        await _collection.InsertOneAsync(ticket);
        return ticket.Id;
    }
}