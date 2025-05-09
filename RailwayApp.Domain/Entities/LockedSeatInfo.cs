using MongoDB.Bson.Serialization.Attributes;

namespace RailwayApp.Domain.Entities;

public class LockedSeatInfo
{
    public Guid ConcreteRouteId { get; set; }
    
    public int StartSegmentNumber { get; set; }

    public int EndSegmentNumber { get; set; }

    public Guid CarriageTemplateId { get; set; }

    public int SeatNumber { get; set; }
}