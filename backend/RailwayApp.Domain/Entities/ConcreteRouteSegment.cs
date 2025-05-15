using MongoDB.Bson.Serialization.Attributes;

namespace RailwayApp.Domain.Entities;

// связь с абстрактным сегментом по abstractsegmentid
// связь с конкретным маршрутом по routeId
// номер сегмента в abstractsegmentid
public class ConcreteRouteSegment : IEntity<Guid>
{
    public Guid AbstractSegmentId { get; set; }
    public Guid ConcreteRouteId { get; set; }
    public int SegmentNumber { get; set; }
    public Guid FromStationId { get; set; }
    public Guid ToStationId { get; set; }
    public DateTime ConcreteDepartureDate { get; set; }
    public DateTime ConcreteArrivalDate { get; set; }

    [BsonId] public Guid Id { get; set; } = Guid.NewGuid();

    // connect to carriageAvailability through CarriageAvailability.ConcreteRouteSegmentId
//    public List<CarriageAvailability> AvailableSeats { get; set; } = new List<CarriageAvailability>();
}