using MongoDB.Bson.Serialization.Attributes;

namespace RailwayApp.Domain.Entities;

public class Train : IEntity<string>
{
    [BsonId] public string Id { get; set; }
    public Guid TrainTypeId { get; set; }
}