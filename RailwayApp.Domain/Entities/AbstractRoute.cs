namespace RailwayApp.Domain.Entities;

/// <summary>
/// связь с поездом через TrainNumber
/// связь с сегментами через List<AbstractRouteSegment>
/// </summary>

public class AbstractRoute
{
    public Guid Id { get; set; }
    public string TrainNumber { get; set; }

    public List<DayOfWeek> ActiveDays { get; set; } = new();
    
    public decimal TransferCost { get; set; }
    public bool HasBeddingOption { get; set; }
    
    public List<AbstractRouteSegment> Segments { get; set; } = new();
}