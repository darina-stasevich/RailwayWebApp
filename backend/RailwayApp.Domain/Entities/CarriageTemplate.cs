namespace RailwayApp.Domain.Entities;

public class CarriageTemplate : IEntity<Guid>
{
    public Guid TrainTypeId { get; set; }
    public int CarriageNumber { get; set; }
    public required string LayoutIdentifier { get; set; }
    public int TotalSeats { get; set; }
    public decimal PriceMultiplier { get; set; }
    public Guid Id { get; set; } = Guid.NewGuid();
}