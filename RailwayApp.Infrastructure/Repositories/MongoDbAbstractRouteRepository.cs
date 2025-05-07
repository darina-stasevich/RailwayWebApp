using Microsoft.Extensions.Options;
using MongoDB.Driver;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces;
using RailwayApp.Domain.Interfaces.IRepositories;

namespace RailwayApp.Infrastructure.Repositories;

public class MongoDbAbstractRouteRepository(IMongoClient client, IOptions<MongoDbSettings> settings)
    : MongoDbGlobalRepository<AbstractRoute, Guid>(client, settings, "AbstractRoutes"), IAbstractRouteRepository
{
}