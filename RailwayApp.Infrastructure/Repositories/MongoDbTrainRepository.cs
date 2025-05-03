using System.Transactions;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces;
using RailwayApp.Domain.Interfaces.IRepositories;

namespace RailwayApp.Infrastructure.Repositories;

public class MongoDbTrainRepository : ITrainRepository
{
    private readonly IMongoCollection<Train> _collection;
    public MongoDbTrainRepository(IMongoClient client, IOptions<MongoDbSettings> settings)
    {
        var database = client.GetDatabase(settings.Value.DatabaseName);
        _collection = database.GetCollection<Train>("Trains");
        
        var indexKeys = Builders<Train>.IndexKeys.Ascending(t => t.Number);
        _collection.Indexes.CreateOne(new CreateIndexModel<Train>(indexKeys));
    }

    public async Task DeleteAllAsync()
    {
        var deleteResult = await _collection.DeleteManyAsync(FilterDefinition<Train>.Empty);
    }
    
    public async Task<string> CreateAsync(Train train)
    {
        await _collection.InsertOneAsync(train);
        return train.Number;
    }

    public async Task<Train?> GetByIdAsync(string id)
    {
        return (await _collection.FindAsync(x => x.Number == id)).FirstOrDefault();
    }
}