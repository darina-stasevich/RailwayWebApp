namespace RailwayApp.Application.Models.Dto;

public class OccupiedSeatDto
{
    public Guid ConcreteRouteId { get; set; }
    public Guid CarriageTemplateId { get; set; }
    public int StartSegmentNumber { get; set; }
    public int EndSegmentNumber { get; set; }
}