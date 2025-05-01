using System.Runtime.InteropServices.JavaScript;
using RailwayApp.Domain.Statuses;

namespace RailwayApp.Domain.Entities;

// определить формат хранения станций отправления и прибытия
// проверить связь с passengerdata
// связь с concreteRoute по RouteId
/// <summary>
/// Id как идентификатор билета
/// UserAccountId как связь с пользователем
/// TrainNumber как связь с Train
/// RouteId как связь с ConcreteRoute
/// StartSegmentId, EndSegmentId как связь с сегментами в ConcreteRoute.RouteSegments
/// PassengerData как связь с PassengerData
/// 
/// TicketStatus как статус оплаты
/// </summary>
public class Ticket
{
    Guid Id { get; set; }

    Guid RouteId { get; set; }
    Guid UserAccountId { get; set; }
    
    Guid StartSegmentId { get; set; }  
    Guid EndSegmentId { get; set; }      

    DateTime DepartureDate { get; set; }
    decimal Price { get; set; }

    PassengerData PassengerData { get; set; }

    int Carriage { get; set; }
    int Seat { get; set; }
    
    bool HasBedLinenSet  { get; set; }
        
    TicketStatus Status { get; set; }
}