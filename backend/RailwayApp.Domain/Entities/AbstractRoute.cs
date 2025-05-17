using MongoDB.Bson.Serialization.Attributes;

namespace RailwayApp.Domain.Entities;

public class AbstractRoute : IEntity<Guid>
{
    public string TrainNumber { get; set; } // connect with Train
    public List<DayOfWeek> ActiveDays { get; set; } = new();
    public TimeSpan DepartureTime { get; set; }
    public decimal TransferCost { get; set; }
    public bool HasBeddingOption { get; set; }

    public bool IsActive { get; set; } = true;

    [BsonId] public Guid Id { get; set; } = Guid.NewGuid();
    // connection through AbstractRouteSegment.RouteId
    //public List<AbstractRouteSegment> Segments { get; set; } = new();
}