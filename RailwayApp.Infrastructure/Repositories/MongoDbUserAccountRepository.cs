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
    public MongoDbUserAccountRepository(IMongoClient client, IOptions<MongoDbSettings> settings)
    {
        var database = client.GetDatabase(settings.Value.DatabaseName);
        _collection = database.GetCollection<UserAccount>("UserAccounts");
        
        var indexKeys = Builders<UserAccount>.IndexKeys.Ascending(u => u.Email);
        _collection.Indexes.CreateOne(new CreateIndexModel<UserAccount>(indexKeys));
    }
    public async Task DeleteAllAsync()
    {
        var deleteResult = await _collection.DeleteManyAsync(FilterDefinition<UserAccount>.Empty);
    }

    public async Task<UserAccount> GetByUserAccountIdAsync(string email)
    {
        return await _collection.Find(u => u.Email == email).FirstOrDefaultAsync();
    }
    
    public async Task<Guid> CreateAsync(UserAccount user)
    {
        await _collection.InsertOneAsync(user);
        return user.Id;
    }

    public async Task<UserAccount?> GetByIdAsync(Guid id)
    {
        return (await _collection.FindAsync(u => u.Id == id)).FirstOrDefault();
    }

    public async Task<UserAccount?> GetByEmailAsync(string email)
    {
        return (await _collection.FindAsync(u => u.Email == email)).FirstOrDefault();
    }

    public async Task<bool> UpdateAsync(Guid id, UserAccount user)
    {
        var filter = Builders<UserAccount>.Filter.Eq(u => u.Id, id);
        var replaceResult = await _collection.ReplaceOneAsync(filter, user);
        return replaceResult.IsAcknowledged && replaceResult.MatchedCount > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var result = await _collection.DeleteOneAsync(u => u.Id == id);
        return result.IsAcknowledged && result.DeletedCount > 0;
    }
}