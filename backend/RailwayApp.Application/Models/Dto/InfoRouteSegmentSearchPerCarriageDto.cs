namespace RailwayApp.Application.Models.Dto;

public class InfoRouteSegmentSearchPerCarriageDto
{
    public Guid ConcreteRouteId { get; set; }
    public int StartSegmentNumber { get; set; }
    public int EndSegmentNumber { get; set; }
    public Guid CarriageTemplateId { get; set; }
}