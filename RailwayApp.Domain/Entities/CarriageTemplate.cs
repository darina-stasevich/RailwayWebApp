using RailwayApp.Domain.Statuses;

namespace RailwayApp.Domain.Entities;

public class CarriageTemplate
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TrainTypeId { get; set; } // to connect with TrainType
    public int CarriageNumber { get; set; }
    public required string LayoutIdentifier { get; set; }
    public int TotalSeats { get; set; }
    public decimal PriceMultiplier { get; set; }
}