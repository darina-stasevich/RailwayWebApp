using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces;
using RailwayApp.Domain.Interfaces.IRepositories;

namespace RailwayApp.Infrastructure.Repositories;

using MongoDB.Driver;

public class MongoDbUserAccountRepository(IMongoClient client, IOptions<MongoDbSettings> settings)
    : MongoDbGlobalRepository<UserAccount, Guid>(client, settings, "UserAccounts"), IUserAccountRepository
{
    
    public async Task<UserAccount> GetByUserAccountIdAsync(string email)
    {
        return await _collection.Find(u => u.Email == email).FirstOrDefaultAsync();
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

}