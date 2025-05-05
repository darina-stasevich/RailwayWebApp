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
namespace RailwayApp.Infrastructure;

public static class MongoDbExtensions
{
    public static IServiceCollection AddMongoDb(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        try
        {
            services.Configure<MongoDbSettings>(configuration.GetSection("MongoDbSettings"));

            BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));

            services.AddSingleton<IMongoClient>(serviceProvider =>
            {
                var settings = serviceProvider.GetRequiredService<IOptions<MongoDbSettings>>().Value;
                var mongoClient = new MongoClient(settings.ConnectionString);
                var db = mongoClient.GetDatabase("RailwayDB");
                try
                {
                    db.RunCommand<BsonDocument>(new BsonDocument("ping", 1));
                }
                catch (Exception)
                {
                   Console.WriteLine("KHHUGHHUJHYDGHDGHRHJJHYJH"); 
                }

                return mongoClient;
            });

            services.AddRepositories();

            return services;
        }
        catch (Exception ex)
        {
            throw;
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
        return services;
    }
}
