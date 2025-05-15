using Microsoft.Extensions.Options;
using MongoDB.Driver;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces;
using RailwayApp.Domain.Interfaces.IRepositories;

namespace RailwayApp.Infrastructure.Repositories;

public class MongoDbCarriageTemplateRepository(IMongoClient client, IOptions<MongoDbSettings> settings)
    : MongoDbGenericRepository<CarriageTemplate, Guid>(client, settings, "CarriageTemplates"),
        ICarriageTemplateRepository
{
    public async Task<IEnumerable<CarriageTemplate>?> GetByTrainTypeIdAsync(Guid trainTypeId,
        IClientSessionHandle? session = null)
    {
        var filter = Builders<CarriageTemplate>.Filter.Eq(u => u.TrainTypeId, trainTypeId);
        if (session != null)
            return await _collection.Find(session, filter).ToListAsync();
        return await _collection.Find(filter).ToListAsync();
    }
}