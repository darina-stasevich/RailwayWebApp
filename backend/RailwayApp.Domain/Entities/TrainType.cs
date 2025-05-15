using MongoDB.Bson.Serialization.Attributes;

namespace RailwayApp.Domain.Entities;

// name is unique identifier
public class TrainType : IEntity<Guid>
{
    public string TypeName { get; set; }

    [BsonId] public Guid Id { get; set; }
//    public List<CarriageTemplate> CarriageTemplates { get; set; } = new();
}