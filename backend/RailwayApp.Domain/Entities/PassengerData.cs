using MongoDB.Bson.Serialization.Attributes;
using RailwayApp.Domain.Statuses;

namespace RailwayApp.Domain.Entities;

/// <summary>
/// Id как id
///
/// TicketId как связь с id билета
/// </summary>
public class PassengerData
{
    public string Surname { get; set; }
    public string FirstName { get; set; }
    public string? SecondName { get; set; }

    public Gender Gender { get; set; }
    public DateTime BirthDate { get; set; }
    
    public string PassportNumber { get; set; }
}