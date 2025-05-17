using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Statuses;

namespace RailwayApp.Application.Models;

public class TicketDto
{
    public Guid TicketId { get; set; }
    public Guid FromStationId { get; set; }
    public Guid ToStationId { get; set; }
    public DateTime DepartureDate { get; set; }
    public DateTime ArrivalDate { get; set; }
    public decimal Price { get; set; }
    public PassengerData PassengerData { get; set; }
    public int Carriage { get; set; }
    public int Seat { get; set; }
    public bool HasBedLinenSet { get; set; }
    public DateTime PurchaseTime { get; set; }
}