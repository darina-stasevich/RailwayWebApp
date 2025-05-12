using System.Runtime.InteropServices.JavaScript;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using RailwayApp.Domain.Statuses;

namespace RailwayApp.Domain.Entities;

public class SeatLock : IEntity<Guid>
{
    [BsonId]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public Guid UserAccountId { get; set; }

    public List<LockedSeatInfo> LockedSeatInfos { get; set; } = new List<LockedSeatInfo>();

    public DateTime ExpirationTimeUtc { get; set; }
    
    public DateTime CreatedAtTimeUtc { get; set; }
    
    
    [BsonRepresentation(BsonType.String)]
    public SeatLockStatus Status { get; set; }
}