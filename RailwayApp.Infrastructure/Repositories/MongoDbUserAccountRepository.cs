using Microsoft.Extensions.Options;
using MongoDB.Driver;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces;
using RailwayApp.Domain.Interfaces.IRepositories;

namespace RailwayApp.Infrastructure.Repositories;

public class MongoDbUserAccountRepository(IMongoClient client, IOptions<MongoDbSettings> settings)
    : MongoDbGenericRepository<UserAccount, Guid>(client, settings, "UserAccounts"), IUserAccountRepository
{
    public async Task<UserAccount> GetByUserAccountIdAsync(string email)
    {
        return await _collection.Find(u => u.Email == email).FirstOrDefaultAsync();
    }

    public async Task<UserAccount?> GetByEmailAsync(string email)
    {
        return await _collection.Find(u => u.Email == email).FirstOrDefaultAsync();
    }

    public async Task<bool> UpdateAsync(Guid id, UserAccount user)
    {
        var filter = Builders<UserAccount>.Filter.Eq(u => u.Id, id);

        var update = Builders<UserAccount>.Update
            .Set(u => u.BirthDate, user.BirthDate)
            .Set(u => u.Gender, user.Gender)
            .Set(u => u.Status, user.Status)
            .Set(u => u.Surname, user.Surname)
            .Set(u => u.SecondName, user.SecondName)
            .Set(u => u.PhoneNumber, user.PhoneNumber)
            .Set(u => u.Name, user.Name);

        var updateResult = await _collection.UpdateOneAsync(filter, update);

        return updateResult.IsAcknowledged && updateResult.MatchedCount > 0;
    }
}