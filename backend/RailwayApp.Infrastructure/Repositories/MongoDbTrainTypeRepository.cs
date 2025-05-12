using Microsoft.Extensions.Options;
using MongoDB.Driver;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces;
using RailwayApp.Domain.Interfaces.IRepositories;

namespace RailwayApp.Infrastructure.Repositories;

public class MongoDbTrainTypeRepository(IMongoClient client, IOptions<MongoDbSettings> settings)
    : MongoDbGenericRepository<TrainType, Guid>(client, settings, "TrainTypes"), ITrainTypeRepository
{
   
}