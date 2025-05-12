using RailwayApp.Domain.Entities;

namespace RailwayApp.Application.Models;

public class LockedSeatInfoResponse
{
    public Guid ConcreteRouteId { get; set; }
    public string FromStation { get; set; }
    public string ToStation { get; set; }
    public int Carriage { get; set; }
    public int SeatNumber { get; set; }
    public bool HasBedLinenSet { get; set; }
    public PassengerData PassengerData { get; set; }
    public DateTime DepartureDate { get; set; }
    public DateTime ArrivalDate { get; set; }
    public Decimal Price { get; set; }
}