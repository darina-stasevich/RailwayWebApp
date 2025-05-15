using Microsoft.Extensions.Options;
using MongoDB.Driver;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces;
using RailwayApp.Domain.Interfaces.IRepositories;

namespace RailwayApp.Infrastructure.Repositories;

public class MongoDbStationRepository(IMongoClient client, IOptions<MongoDbSettings> settings)
    : MongoDbGenericRepository<Station, Guid>(client, settings, "Stations"), IStationRepository
{
    public async Task<Station?> GetByNameAsync(string name)
    {
        return await _collection.Find(s => s.Name == name).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Station>> GetByIdsAsync(List<Guid> ids)
    {
        var filter = Builders<Station>.Filter.In(s => s.Id, ids);
        return await _collection.Find(filter).ToListAsync();
    }
}