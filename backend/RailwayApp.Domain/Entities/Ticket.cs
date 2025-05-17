using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using RailwayApp.Domain.Statuses;

namespace RailwayApp.Domain.Entities;

// определить формат хранения станций отправления и прибытия
// проверить связь с passengerdata
// связь с concreteRoute по RouteId
/// <summary>
///     Id как идентификатор билета
///     UserAccountEmail как связь с пользователем
///     TrainNumber как связь с Train
///     RouteId как связь с ConcreteRoute
///     StartSegmentId, EndSegmentId как связь с сегментами в ConcreteRoute.RouteSegments
///     PassengerData как связь с PassengerData
///     TicketStatus как статус оплаты
/// </summary>
public class Ticket : IEntity<Guid>
{
    public Guid RouteId { get; set; } // concreteRoute
    public Guid UserAccountId { get; set; }

    public int StartSegmentNumber { get; set; } // concreteRouteSegment
    public int EndSegmentNumber { get; set; } // concreteRouteSegment

    public DateTime DepartureDate { get; set; }
    public DateTime ArrivalDate { get; set; }
    public decimal Price { get; set; }

    public PassengerData PassengerData { get; set; } // PassengerData

    public int Carriage { get; set; }
    public int Seat { get; set; }
    public bool HasBedLinenSet { get; set; }

    public DateTime PurchaseTime { get; set; }

    [BsonRepresentation(BsonType.String)]
    public TicketStatus Status { get; set; }

    [BsonId] public Guid Id { get; set; } = Guid.NewGuid();
}