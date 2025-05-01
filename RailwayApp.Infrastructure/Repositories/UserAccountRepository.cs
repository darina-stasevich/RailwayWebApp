using Microsoft.Extensions.Options;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces;
using RailwayApp.Domain.Interfaces.IRepositories;

namespace RailwayApp.Infrastructure.Repositories;

using MongoDB.Driver;

public class UserAccountRepository : IUserAccountRepository
{
    private readonly IMongoCollection<UserAccount> _collection;

    public UserAccountRepository(IMongoClient client, IOptions<MongoDbSettings> settings)
    {
        var database = client.GetDatabase(settings.Value.DatabaseName);
        _collection = database.GetCollection<UserAccount>("UserAccounts");
        
        // Создание уникального индекса на Email (хотя [BsonId] уже обеспечивает уникальность)
        var indexKeys = Builders<UserAccount>.IndexKeys.Ascending(u => u.Email);
        _collection.Indexes.CreateOne(new CreateIndexModel<UserAccount>(indexKeys));
    }

    public async Task<UserAccount> GetByEmailAsync(string email)
    {
        return await _collection.Find(u => u.Email == email).FirstOrDefaultAsync();
    }


    public async Task CreateAsync(UserAccount user)
    {
        await _collection.InsertOneAsync(user);
    }
}