using Microsoft.Extensions.Options;
using MongoDB.Driver;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces;
using RailwayApp.Domain.Interfaces.IRepositories;

namespace RailwayApp.Infrastructure.Repositories;

public class MongoDbConcreteRouteRepository(IMongoClient client, IOptions<MongoDbSettings> settings)
    : MongoDbGenericRepository<ConcreteRoute, Guid>(client, settings, "ConcreteRoutes"), IConcreteRouteRepository
{
    public async Task<IEnumerable<ConcreteRoute>> GetConcreteRoutesInDate(DateTime startDate, DateTime endDate)
    {
        return await _collection.Find(r => r.RouteDepartureDate >= startDate && r.RouteDepartureDate < endDate).ToListAsync();
    }
}