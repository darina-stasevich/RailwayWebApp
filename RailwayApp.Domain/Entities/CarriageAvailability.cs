using System.Collections;

namespace RailwayApp.Domain.Entities;

public class CarriageAvailability
{
    public Guid CarriageTemplateId { get; set; }
    public BitArray OccupiedSeats { get; set; }  // Битовая маска занятых мест
    
}