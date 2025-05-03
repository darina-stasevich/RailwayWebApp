namespace RailwayApp.Application.Models;

public class ScheduleDto
{ 
    public Guid ConcreteRouteId { get; set; }
    public string TrainNumber { get; set; } = string.Empty;
    public DateOnly DepartureDate { get; set; }
    public List<SegmentScheduleDto> Segments { get; set; } = new List<SegmentScheduleDto>();

}