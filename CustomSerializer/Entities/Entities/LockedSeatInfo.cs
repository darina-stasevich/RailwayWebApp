namespace RailwayApp.Domain.Entities;

public class LockedSeatInfo
{
    public Guid ConcreteRouteId { get; set; }
    public int StartSegmentNumber { get; set; }
    public int EndSegmentNumber { get; set; }
    public Guid CarriageTemplateId { get; set; }
    public int Carriage { get; set; }
    public int SeatNumber { get; set; }
    public bool HasBedLinenSet { get; set; }
    public PassengerData PassengerData { get; set; }
    public DateTime DepartureDateUtc { get; set; }
    public DateTime ArrivalDateUtc { get; set; }
    public decimal Price { get; set; }
}