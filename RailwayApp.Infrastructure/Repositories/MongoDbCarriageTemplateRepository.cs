using Microsoft.Extensions.Options;
using MongoDB.Driver;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces;
using RailwayApp.Domain.Interfaces.IRepositories;

namespace RailwayApp.Infrastructure.Repositories;

public class MongoDbCarriageTemplateRepository : ICarriageTemplateRepository
{
    private readonly IMongoCollection<CarriageTemplate> _collection;
    
    public MongoDbCarriageTemplateRepository(IMongoClient client, IOptions<MongoDbSettings> settings)
    {
        var database = client.GetDatabase(settings.Value.DatabaseName);
        _collection = database.GetCollection<CarriageTemplate>("CarriageTemplates");
    }
    
    public async Task DeleteAllAsync()
    {
        var deleteResult = await _collection.DeleteManyAsync(FilterDefinition<CarriageTemplate>.Empty);
    }
    
    public async Task<Guid> CreateAsync(CarriageTemplate carriageTemplate)
    {
        await _collection.InsertOneAsync(carriageTemplate);
        return carriageTemplate.Id;
    }

    public async Task<CarriageTemplate?> GetByIdAsync(Guid id)
    {
        return (await _collection.FindAsync(t => t.Id == id)).FirstOrDefault();
    }

    public async Task<List<CarriageTemplate>> GetByTrainTypeIdAsync(Guid trainTypeId)
    {
        return (await _collection.FindAsync(t => t.TrainTypeId == trainTypeId)).ToList();
    }
}