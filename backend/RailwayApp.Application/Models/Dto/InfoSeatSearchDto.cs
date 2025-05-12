namespace RailwayApp.Application.Models.Dto;

public class InfoSeatSearchDto
{ 
    public Guid ConcreteRouteId { get; set; }
    public int StartSegmentNumber { get; set; }
    public int EndSegmentNumber { get; set; }
    public Guid CarriageTemplateId { get; set; }
    public int SeatNumber { get; set; }
}