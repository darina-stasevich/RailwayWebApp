using MongoDB.Bson.Serialization.Attributes;

namespace RailwayApp.Domain.Entities;

public class ConcreteRoute : IEntity<Guid>
{
    [BsonId]
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AbstractRouteId { get; set; }
    public DateTime RouteDepartureDate { get; set; }
    
    // connect to segments through ConcreteRouteSegment.ConcreteRouteId
    //public List<ConcreteRouteSegment> Segments { get; set; } = new();
}