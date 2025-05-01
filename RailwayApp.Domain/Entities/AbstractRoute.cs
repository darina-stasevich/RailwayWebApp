using System.Collections;
using MongoDB.Bson.Serialization.Attributes;

namespace RailwayApp.Domain.Entities;

/// <summary>
/// связь с поездом через TrainNumber
/// связь с сегментами через List<AbstractRouteSegment>
/// </summary>

public class AbstractRoute
{
    [BsonId]
    public Guid Id { get; set; } = Guid.NewGuid();
    public string TrainNumber { get; set; } // connect with Train

    public string ActiveDays { get; set; }
    //public List<DayOfWeek> ActiveDays { get; set; } = new();
    
    public decimal TransferCost { get; set; }
    public bool HasBeddingOption { get; set; }
    
    // connection through AbstractRouteSegment.RouteId
    //public List<AbstractRouteSegment> Segments { get; set; } = new();
}