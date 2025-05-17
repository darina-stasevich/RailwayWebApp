using MongoDB.Bson.Serialization.Attributes;

namespace RailwayApp.Domain.Entities;

public class AbstractRouteSegment : IEntity<Guid>
{
    public Guid AbstractRouteId { get; set; }

    public int SegmentNumber { get; set; }

    public Guid FromStationId { get; set; }
    public Guid ToStationId { get; set; }

    public TimeSpan FromTime { get; set; }
    public TimeSpan ToTime { get; set; }

    public decimal SegmentCost { get; set; }

    [BsonId] public Guid Id { get; set; } = Guid.NewGuid();
}