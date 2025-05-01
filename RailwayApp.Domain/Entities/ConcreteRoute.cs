namespace RailwayApp.Domain.Entities;

// связь с абстрактным маршрутом по id
// связь с сегментами
// нужна ли связь с билетами?

public class ConcreteRoute
{
    public Guid Id { get; set; }
    public Guid AbstractRouteId { get; set; }
    public DateTime RouteDepartureDate { get; set; }
    public List<ConcreteRouteSegment> Segments { get; set; } = new();
}