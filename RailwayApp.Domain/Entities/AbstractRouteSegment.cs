using MongoDB.Bson.Serialization.Attributes;

namespace RailwayApp.Domain.Entities;

/// <summary>
/// Связь с абстрактным маршрутом по RouteId
/// связь со станциями через FromStationId и ToStationId
/// </summary>
public class AbstractRouteSegment
{
    [BsonId]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public Guid AbstractRouteId { get; set; }
    
    public int SegmentNumber { get; set; }
    
    public Guid FromStationId { get; set; }
    public Guid ToStationId { get; set; }
    
    public DateTime FromTime { get; set; }
    public DateTime ToTime { get; set; }
    
    public decimal SegmentCost { get; set; }
}