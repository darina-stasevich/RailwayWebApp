using MongoDB.Bson.Serialization.Attributes;

namespace RailwayApp.Domain.Entities;

// связь с абстрактным сегментом по abstractsegmentid
// связь с конкретным маршрутом по routeId
// номер сегмента в abstractsegmentid
public class ConcreteRouteSegment
{
    [BsonId]
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AbstractSegmentId { get; set; }
    public Guid ConcreteRouteId { get; set; }
    public DateTime ConcreteDepartureDate { get; set; }
    public DateTime ConcreteArrivalDate { get; set; }
    
    
    // connect to carriageAvailability through CarriageAvailability.ConcreteRouteSegmentId
//    public List<CarriageAvailability> AvailableSeats { get; set; } = new List<CarriageAvailability>();
}