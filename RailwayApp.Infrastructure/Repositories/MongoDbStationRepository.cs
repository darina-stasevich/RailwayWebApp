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
    
    public MongoDbStationRepository(IMongoClient client, IOptions<MongoDbSettings> settings)
    {
        
        var database = client.GetDatabase(settings.Value.DatabaseName);
        _collection = database.GetCollection<Station>("Stations");
    }
    public async Task DeleteAllAsync()
    {
        var deleteResult = await _collection.DeleteManyAsync(FilterDefinition<Station>.Empty);
    }
    
    public async Task<Guid> CreateAsync(Station station)
    {
        await _collection.InsertOneAsync(station);
        return station.Id;
    }

    public async Task<Station?> GetByNameAsync(string name)
    {
        return (await _collection.FindAsync(s => s.Name == name)).FirstOrDefault();
    }

    public async Task<Station?> GetByIdAsync(Guid id)
    {
        return (await _collection.FindAsync(s => s.Id == id)).FirstOrDefault();
    }

    public async Task<List<Station>> GetAllAsync()
    {
        return (await _collection.FindAsync(s => true)).ToList();
    }
}