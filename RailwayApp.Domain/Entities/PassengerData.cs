using RailwayApp.Domain.Statuses;

namespace RailwayApp.Domain.Entities;

/// <summary>
/// Id как id
///
/// TicketId как связь с id билета
/// </summary>
public class PassengerData
{
    public Guid Id { get; set; }
    
    public Guid TicketId { get; set; }
    
    string Surname { get; set; }
    string FirstName { get; set; }
    string? SecondName { get; set; }

    Gender Gender { get; set; }
    DateTime BirthDate { get; set; }
    
    string PassportNumber { get; set; }
}