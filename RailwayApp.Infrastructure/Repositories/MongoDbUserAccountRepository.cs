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
        try
        {
            _logger.LogInformation($"GetByEmailAsync: {email}", email);
            return await _collection.Find(u => u.Email == email).FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"GetByEmailAsync: {email}", email);
            throw;
        }
    }


    public async Task CreateAsync(UserAccount user)
    {
        try
        {
            _logger.LogInformation($"CreateAsync: {user.Email}", user.Email);
            await _collection.InsertOneAsync(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"CreateAsync: {user.Email}", user.Email);
        }
    }
}