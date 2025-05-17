using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using RailwayApp.Domain.Statuses;

namespace RailwayApp.Domain.Entities;

public class Ticket : IEntity<Guid>
{
    public Guid RouteId { get; set; }
    public Guid UserAccountId { get; set; }
    public int StartSegmentNumber { get; set; }
    public int EndSegmentNumber { get; set; }
    public DateTime DepartureDate { get; set; }
    public DateTime ArrivalDate { get; set; }
    public decimal Price { get; set; }
    public PassengerData PassengerData { get; set; }
    public int Carriage { get; set; }
    public int Seat { get; set; }
    public bool HasBedLinenSet { get; set; }
    public DateTime PurchaseTime { get; set; }
    
    [BsonRepresentation(BsonType.String)]
    public TicketStatus Status { get; set; }
    [BsonId] public Guid Id { get; set; } = Guid.NewGuid();
}