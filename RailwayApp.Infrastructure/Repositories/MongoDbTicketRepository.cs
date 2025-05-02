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
    private readonly ILogger<MongoDbTicketRepository> _logger;
    public MongoDbTicketRepository(IMongoClient client, IOptions<MongoDbSettings> settings, ILogger<MongoDbTicketRepository> logger)
    {
        _logger = logger;
        
        var database = client.GetDatabase(settings.Value.DatabaseName);
        _collection = database.GetCollection<Ticket>("Tickets");
        
        var indexKeys = Builders<Ticket>.IndexKeys.Ascending(t => t.UserAccountEmail);
        _collection.Indexes.CreateOne(new CreateIndexModel<Ticket>(indexKeys));
    }

    public async Task<IEnumerable<Ticket>> GetByUserEmailAsync(string email)
    {
        try
        {
            _logger.LogInformation($"GetByUserEmailAsync: {email}", email);
            return await _collection.Find(t => t.UserAccountEmail == email).ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"GetByUserEmailAsync: {email}", email);
            throw;
        }
    }

    public async Task<Ticket> GetByIdAsync(Guid id)
    {
        try
        {
            _logger.LogInformation($"GetByIdAsync: {id}", id);
            return await _collection.Find(t => t.Id == id).FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"GetByIdAsync: {id}", id);
            throw;
        }
    }

    public async Task CreateAsync(Ticket ticket)
    {
        try
        {
            await _collection.InsertOneAsync(ticket);
            _logger.LogInformation($"CreateAsync: {ticket.Id}", ticket.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"CreateAsync: {ticket.Id}", ticket.Id);
        }
    }
}