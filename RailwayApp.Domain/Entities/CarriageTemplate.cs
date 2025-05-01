using RailwayApp.Domain.Statuses;

namespace RailwayApp.Domain.Entities;

public class CarriageTemplate
{
    public Guid Id { get; set; }
    public string TrainTypeName { get; set; }
    public int CarriageNumber { get; set; }
    public CarriageType Type { get; set; }
    public int TotalSeats { get; set; }
}