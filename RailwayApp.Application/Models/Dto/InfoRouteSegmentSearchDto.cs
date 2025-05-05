namespace RailwayApp.Application.Models.Dto;

public class InfoRouteSegmentSearchDto
{
    public Guid ConcreteRouteId { get; set; }
    public int StartSegmentNumber { get; set; }
    public int EndSegmentNumber { get; set; }
}