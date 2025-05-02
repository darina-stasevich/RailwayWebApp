using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces;
using RailwayApp.Domain.Interfaces.IRepositories;

namespace RailwayApp.Infrastructure.Repositories;

public class MongoDbStationRepository : IStationRepository
{ 
    private readonly IMongoCollection<Station> _collection;
    private readonly ILogger<MongoDbStationRepository> _logger;
    
    public MongoDbStationRepository(IMongoClient client, IOptions<MongoDbSettings> settings, ILogger<MongoDbStationRepository> logger)
    {
        _logger = logger;
        
        var database = client.GetDatabase(settings.Value.DatabaseName);
        _collection = database.GetCollection<Station>("Stations");
    }
    
    public async Task<Guid> CreateAsync(Station station)
    {
        try
        {
            await _collection.InsertOneAsync(station);
            _logger.LogInformation($"Created station: {station.Name}", station.Name);
            return station.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error Create station {station.Name}", station.Name);
            throw;
        }
    }

    public async Task<Station?> GetByNameAsync(string name)
    {
        return (await _collection.FindAsync(s => s.Name == name)).FirstOrDefault();
    }

    public async Task<List<Station>> GetAllAsync()
    {
        return (await _collection.FindAsync(s => true)).ToList();
    }
}