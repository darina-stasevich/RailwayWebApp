using System.Collections;
using MongoDB.Bson.Serialization.Attributes;

namespace RailwayApp.Domain.Entities;

// ? добавить Id?
public class CarriageAvailability
{
    [BsonId]
    public Guid Id { get; set; }
    public Guid ConcreteRouteSegmentId { get; set; } // to connect with concreteRouteSegment
    public Guid CarriageTemplateId { get; set; }
    public BitArray OccupiedSeats { get; set; }  // Битовая маска занятых мест, 0 для занятых, 1 для свободных
    
}