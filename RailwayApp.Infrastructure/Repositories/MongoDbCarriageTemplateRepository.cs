using Microsoft.Extensions.Options;
using MongoDB.Driver;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces;
using RailwayApp.Domain.Interfaces.IRepositories;

namespace RailwayApp.Infrastructure.Repositories;

public class MongoDbCarriageTemplateRepository(IMongoClient client, IOptions<MongoDbSettings> settings)
    : MongoDbGlobalRepository<CarriageTemplate, Guid>(client, settings, "CarriageTemplates"), ICarriageTemplateRepository
{
    public async Task<List<CarriageTemplate>?> GetByTrainTypeIdAsync(Guid trainTypeId)
    {
        return (await _collection.FindAsync(t => t.TrainTypeId == trainTypeId)).ToList();
    }
}