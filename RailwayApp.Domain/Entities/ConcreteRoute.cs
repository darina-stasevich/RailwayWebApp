using MongoDB.Bson.Serialization.Attributes;

namespace RailwayApp.Domain.Entities;

// связь с абстрактным маршрутом по id
// связь с сегментами
// нужна ли связь с билетами?

public class ConcreteRoute
{
    [BsonId]
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AbstractRouteId { get; set; }
    public DateTime RouteDepartureDate { get; set; }
    
    // connect to segments through ConcreteRouteSegment.ConcreteRouteId
    //public List<ConcreteRouteSegment> Segments { get; set; } = new();
}