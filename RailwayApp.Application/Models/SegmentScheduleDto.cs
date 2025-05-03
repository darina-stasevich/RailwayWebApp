namespace RailwayApp.Application.Models;

public class SegmentScheduleDto
{
    public string FromStation { get; set; } = string.Empty;
    public string ToStation { get; set; } = string.Empty;
    
    public DateTime DepartureDate { get; set; }
    public DateTime ArrivalDate { get; set; }
}