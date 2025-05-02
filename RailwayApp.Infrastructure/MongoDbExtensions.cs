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
        
        services.Configure<MongoDbSettings>(configuration.GetSection("MongoDbSettings"));

        BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));

        services.AddSingleton<IMongoClient>(serviceProvider =>
        {
            var settings = serviceProvider.GetRequiredService<IOptions<MongoDbSettings>>().Value;
            return new MongoClient(settings.ConnectionString);
        });
        
        services.AddRepositories();
        return services;
    }

    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUserAccountRepository, MongoDbUserAccountRepository>();
        services.AddScoped<ITicketRepository, MongoDbTicketRepository>();
        return services;
    }
}
