using MongoDB.Bson.Serialization.Attributes;
using RailwayApp.Domain.Statuses;

namespace RailwayApp.Domain.Entities;

/// <summary>
/// Number как идентификатор
/// Type для фронта
/// 
/// CarriageTemplates для формата вагонов, хз зачем
/// 
/// </summary>
public class Train
{
    [BsonId]
    public string Number { get; set; } // key
    public Guid TrainTypeId { get; set; }
}
