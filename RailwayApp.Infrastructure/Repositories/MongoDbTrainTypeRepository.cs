using Microsoft.Extensions.Options;
using MongoDB.Driver;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces;
using RailwayApp.Domain.Interfaces.IRepositories;

namespace RailwayApp.Infrastructure.Repositories;

public class MongoDbTrainTypeRepository : ITrainTypeRepository
{
    private readonly IMongoCollection<TrainType> _collection;
    public MongoDbTrainTypeRepository(IMongoClient client, IOptions<MongoDbSettings> settings)
    {
        var database = client.GetDatabase(settings.Value.DatabaseName);
        _collection = database.GetCollection<TrainType>("TrainTypes");
        
        var indexKeys = Builders<TrainType>.IndexKeys.Ascending(t => t.TypeName);
        _collection.Indexes.CreateOne(new CreateIndexModel<TrainType>(indexKeys));
    }
    
    public async Task DeleteAllAsync()
    {
        var deleteResult = await _collection.DeleteManyAsync(FilterDefinition<TrainType>.Empty);
    }
    
    public async Task<Guid> CreateAsync(TrainType trainType)
    {
        await _collection.InsertOneAsync(trainType);
        return trainType.Id;
    }

    public async Task<TrainType?> GetByIdAsync(Guid id)
    {
        return (await _collection.FindAsync(x => x.Id == id)).FirstOrDefault();
    }
}