using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces;
using RailwayApp.Domain.Interfaces.IRepositories;

namespace RailwayApp.Infrastructure.Repositories;

using MongoDB.Driver;

public class MongoDbUserAccountRepository : IUserAccountRepository
{
    private readonly IMongoCollection<UserAccount> _collection;
    private readonly ILogger<MongoDbUserAccountRepository> _logger;
    public MongoDbUserAccountRepository(IMongoClient client, IOptions<MongoDbSettings> settings, ILogger<MongoDbUserAccountRepository> logger)
    {
        _logger = logger;
        
        var database = client.GetDatabase(settings.Value.DatabaseName);
        _collection = database.GetCollection<UserAccount>("UserAccounts");
        
        var indexKeys = Builders<UserAccount>.IndexKeys.Ascending(u => u.Email);
        _collection.Indexes.CreateOne(new CreateIndexModel<UserAccount>(indexKeys));
    }

    public async Task<UserAccount> GetByEmailAsync(string email)
    {
        return await _collection.Find(u => u.Email == email).FirstOrDefaultAsync();
    }


    public async Task<string> CreateAsync(UserAccount user)
    {
        try
        {
            _logger.LogInformation($"CreateAsync: {user.Email}", user.Email);
            await _collection.InsertOneAsync(user);
            return user.Email;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"CreateAsync: {user.Email}", user.Email);
            throw;
        }
    }
}