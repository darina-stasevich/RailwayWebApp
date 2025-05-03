namespace RailwayApp.Application.Models;

public class ComplexRouteDto
{
    public List<DirectRouteDto> DirectRoutes { get; set; } = new List<DirectRouteDto>();
    public decimal TotalCost { get; set; }
    public DateTime DepartureDate { get; set; }
    public DateTime ArrivalDate { get; set; }
    public TimeSpan TotalDuration { get; set; }
}