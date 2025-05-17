using System.Collections;
using MongoDB.Bson.Serialization.Attributes;

namespace RailwayApp.Domain.Entities;

public class CarriageAvailability : IEntity<Guid>
{
    public Guid ConcreteRouteSegmentId { get; set; }
    public Guid CarriageTemplateId { get; set; }
    public BitArray OccupiedSeats { get; set; } // Битовая маска занятых мест, 0 для занятых, 1 для свободных

    [BsonId] public Guid Id { get; set; }
}