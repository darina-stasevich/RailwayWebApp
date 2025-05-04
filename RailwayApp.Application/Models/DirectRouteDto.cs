namespace RailwayApp.Application.Models;

public class DirectRouteDto
{
    public Guid ConcreteRouteId { get; set; }
    public int StartSegmentNumber { get; set; }
    public int EndSegmentNumber { get; set; }
    public DateTime DepartureDate { get; set; }
    public DateTime ArrivalDate { get; set; }
    public TimeSpan TimeInTransit { get; set; }

    public decimal MinimalCost { get; set; }
    public decimal MaximumCost { get; set; }
    public int AvailableSeats { get; set; }
}