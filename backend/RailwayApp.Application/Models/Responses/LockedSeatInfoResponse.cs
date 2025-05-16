using RailwayApp.Domain.Entities;

namespace RailwayApp.Application.Models;

public class LockedSeatInfoResponse
{
    public required Guid ConcreteRouteId { get; set; }
    public required Guid FromStationId { get; set; }
    public required Guid ToStationId { get; set; }
    public required int Carriage { get; set; }
    public required int SeatNumber { get; set; }
    public required bool HasBedLinenSet { get; set; }
    public required PassengerData PassengerData { get; set; }
    public required DateTime DepartureDate { get; set; }
    public required DateTime ArrivalDate { get; set; }
    public decimal Price { get; set; }
}