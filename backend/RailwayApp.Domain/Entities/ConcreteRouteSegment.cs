using MongoDB.Bson.Serialization.Attributes;

namespace RailwayApp.Domain.Entities;

public class ConcreteRouteSegment : IEntity<Guid>
{
    [BsonId] public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AbstractSegmentId { get; set; }
    public Guid ConcreteRouteId { get; set; }
    public int SegmentNumber { get; set; }
    public Guid FromStationId { get; set; }
    public Guid ToStationId { get; set; }
    public DateTime ConcreteDepartureDate { get; set; }
    public DateTime ConcreteArrivalDate { get; set; }
}