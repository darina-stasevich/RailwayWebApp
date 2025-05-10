namespace RailwayApp.Application.Models.Dto;

public class FreeTicketDto
{
    public Guid ConcreteRouteId { get; set; }
    public int StartSegmentNumber { get; set; }
    public int EndSegmentNumber { get; set; }
    public int Carriage { get; set; }
    public int SeatNumber { get; set; }
}