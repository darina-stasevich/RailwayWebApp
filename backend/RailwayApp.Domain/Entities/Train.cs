using MongoDB.Bson.Serialization.Attributes;

namespace RailwayApp.Domain.Entities;

/// <summary>
///     Number как идентификатор
///     Type для фронта
///     CarriageTemplates для формата вагонов, хз зачем
/// </summary>
public class Train : IEntity<string>
{
    public Guid TrainTypeId { get; set; }

    [BsonId] public string Id { get; set; } // key
}