using System.Runtime.InteropServices.JavaScript;
using MongoDB.Bson.Serialization.Attributes;
using RailwayApp.Domain.Statuses;

namespace RailwayApp.Domain.Entities;

// определить формат хранения станций отправления и прибытия
// проверить связь с passengerdata
// связь с concreteRoute по RouteId
/// <summary>
/// Id как идентификатор билета
/// UserAccountEmail как связь с пользователем
/// TrainNumber как связь с Train
/// RouteId как связь с ConcreteRoute
/// StartSegmentId, EndSegmentId как связь с сегментами в ConcreteRoute.RouteSegments
/// PassengerData как связь с PassengerData
/// 
/// TicketStatus как статус оплаты
/// </summary>
public class Ticket
{
    [BsonId]
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid RouteId { get; set; } // concreteRoute
    public string UserAccountEmail { get; set; }
    
    public Guid StartSegmentId { get; set; }   // concreteRouteSegment
    public Guid EndSegmentId { get; set; }     // concreteRouteSegment

    public DateTime DepartureDate { get; set; }
    public decimal Price { get; set; }

    public Guid PassengerDataId { get; set; }   // PassengerData
    public int Carriage { get; set; }
    public int Seat { get; set; }
    public bool HasBedLinenSet  { get; set; }
        
    public TicketStatus Status { get; set; }
}