using System.Transactions;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces;
using RailwayApp.Domain.Interfaces.IRepositories;

namespace RailwayApp.Infrastructure.Repositories;

public class MongoDbTrainRepository(IMongoClient client, IOptions<MongoDbSettings> settings)
    : MongoDbGenericRepository<Train, string>(client, settings, "Trains"), ITrainRepository
{
    
}