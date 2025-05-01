namespace RailwayApp.Domain.Entities;

// связь с абстрактным сегментом по abstractsegmentid
// связь с конкретным маршрутом по routeId
// номер сегмента в abstractsegmentid
public class ConcreteRouteSegment
{
    public Guid Id { get; set; }
    public Guid AbstractSegmentId { get; set; }
    public Guid RouteId { get; set; }
    public DateTime SegmentDepartureDate { get; set; }
    public List<CarriageAvailability> AvailableSeats { get; set; } = new List<CarriageAvailability>();
}