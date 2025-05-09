using System.Net.NetworkInformation;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using RailwayApp.Domain.Interfaces;
using RailwayApp.Domain.Interfaces.IRepositories;
using RailwayApp.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Statuses;

namespace RailwayApp.Infrastructure;

public static class MongoDbExtensions
{
    public static IServiceCollection AddMongoDb(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<MongoDbSettings>(configuration.GetSection("MongoDbSettings"));

        BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));
        BsonSerializer.RegisterSerializer(new EnumSerializer<SeatLockStatus>(BsonType.String));
        
        services.AddSingleton<IMongoClient>(serviceProvider =>
        {
            var settings = serviceProvider.GetRequiredService<IOptions<MongoDbSettings>>().Value;
            var mongoClient = new MongoClient(settings.ConnectionString);
            return mongoClient;
        });

        var serviceProviderForIndex = services.BuildServiceProvider();
        var mongoClientForIndex = serviceProviderForIndex.GetRequiredService<IMongoClient>();
        var mongoSettingsForIndex = serviceProviderForIndex.GetRequiredService<IOptions<MongoDbSettings>>().Value;

        
        EnsureIndexAsync<SeatLock>(
                mongoClientForIndex,
                mongoSettingsForIndex.DatabaseName,
                "SeatLocks",
                Builders<SeatLock>.IndexKeys.Ascending(sl => sl.ExpirationTimeUtc),
                new CreateIndexOptions
                {
                    Name = "SeatLock_ExpirationTimeUtc_TTL",
                    ExpireAfter = TimeSpan.FromSeconds(0)
                })
            .GetAwaiter().GetResult();

        
        services.AddRepositories();

        return services;
        
    }
    
    private static async Task EnsureIndexAsync<TDocument>(
        IMongoClient mongoClient,
        string databaseName,
        string collectionName,
        IndexKeysDefinition<TDocument> keysDefinition,
        CreateIndexOptions indexOptions)
    {
        if (string.IsNullOrWhiteSpace(databaseName))
        {
            Console.WriteLine($"Error: DatabaseName is not configured for index creation for collection '{collectionName}'.");
            return;
        }

        var database = mongoClient.GetDatabase(databaseName);
        var collection = database.GetCollection<TDocument>(collectionName);

        try
        {
            bool indexExists = false;
            using (var cursor = await collection.Indexes.ListAsync())
            {
                var indexes = await cursor.ToListAsync();
                if (indexes.Any(idx => idx["name"] == indexOptions.Name))
                {
                    indexExists = true;
                }
            }

            if (!indexExists)
            {
                await collection.Indexes.CreateOneAsync(new CreateIndexModel<TDocument>(keysDefinition, indexOptions));
                Console.WriteLine($"Index '{indexOptions.Name}' for collection '{collectionName}' created successfully.");
            }
            else
            {
                Console.WriteLine($"Index '{indexOptions.Name}' for collection '{collectionName}' already exists.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during index ('{indexOptions.Name}') creation/check for collection '{collectionName}': {ex.Message}");
        }
    }


    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUserAccountRepository, MongoDbUserAccountRepository>();
        services.AddScoped<ITicketRepository, MongoDbTicketRepository>();
        services.AddScoped<IStationRepository, MongoDbStationRepository>();
        services.AddScoped<IAbstractRouteRepository, MongoDbAbstractRouteRepository>();
        services.AddScoped<IAbstractRouteSegmentRepository, MongoDbAbstractRouteSegmentRepository>();
        services.AddScoped<ICarriageAvailabilityRepository, MongoDbCarriageAvailabilityRepository>();
        services.AddScoped<ICarriageTemplateRepository, MongoDbCarriageTemplateRepository>();
        services.AddScoped<IConcreteRouteSegmentRepository, MongoDbConcreteRouteSegmentRepository>();
        services.AddScoped<IConcreteRouteRepository, MongoDbConcreteRouteRepository>();
        services.AddScoped<ITrainRepository, MongoDbTrainRepository>();
        services.AddScoped<ITrainTypeRepository, MongoDbTrainTypeRepository>();
        services.AddScoped<ISeatLockRepository, MongoDbSeatLockRepository>();
        return services;
    }
}
