namespace RailwayApp.Application.Models;

public class ComplexRouteDto
{
    public List<DirectRouteDto> DirectRoutes { get; set; } = new();
    public decimal MinimalTotalCost { get; set; }
    public decimal MaximumTotalCost { get; set; }
    public DateTime DepartureDate { get; set; }
    public DateTime ArrivalDate { get; set; }
    public TimeSpan TotalDuration { get; set; }
}