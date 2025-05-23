using MongoDB.Bson.Serialization.Attributes;

namespace RailwayApp.Domain.Entities;

// name is unique identifier
public class TrainType : IEntity<Guid>
{
    [BsonId] public Guid Id { get; set; }
    public string TypeName { get; set; }
}