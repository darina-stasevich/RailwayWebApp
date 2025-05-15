using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using RailwayApp.Domain.Statuses;

namespace RailwayApp.Domain.Entities;

public class SeatLock : IEntity<Guid>
{
    public Guid UserAccountId { get; set; }

    public List<LockedSeatInfo> LockedSeatInfos { get; set; } = new();

    public DateTime ExpirationTimeUtc { get; set; }

    public DateTime CreatedAtTimeUtc { get; set; }


    [BsonRepresentation(BsonType.String)] public SeatLockStatus Status { get; set; }

    [BsonId] public Guid Id { get; set; } = Guid.NewGuid();
}