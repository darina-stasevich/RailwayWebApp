using MongoDB.Bson.Serialization.Attributes;

namespace RailwayApp.Domain.Entities;

public class ConcreteRoute : IEntity<Guid>
{
    public Guid AbstractRouteId { get; set; }
    public DateTime RouteDepartureDate { get; set; }

    [BsonId] public Guid Id { get; set; } = Guid.NewGuid();
}