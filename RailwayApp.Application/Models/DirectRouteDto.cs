namespace RailwayApp.Application.Models;

public class DirectRouteDto
{
    public Guid ConcreteRouteId { get; set; }
    
    public Guid FromStationId { get; set; }
    public Guid ToStationId { get; set; }
    
    public DateTime DepartureDate { get; set; }
    public DateTime ArrivalDate { get; set; }
    public TimeSpan TimeInTransit { get; set; }

    public decimal Cost { get; set; }
    public int AvailableSeats { get; set; }
}