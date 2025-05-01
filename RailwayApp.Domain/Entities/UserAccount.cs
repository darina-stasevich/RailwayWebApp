using RailwayApp.Domain.Statuses;

namespace RailwayApp.Domain.Entities;

// поставить email как index,
// разобраться с Id

public class UserAccount
{
    string Email { get; set; }
    
    string Surname { get; set; }
    string Name { get; set; }
    string? SecondName { get; set; }
    
    string? PhoneNumber { get; set; }
    
    DateTime BirthDate { get; set; }
    
    string? PassportNumber { get; set; }
    Gender? Gender { get; set; }
    
    string HashedPassword { get; set; }
    UserAccountStatus Status { get; set; }
    
    List<Ticket> Tickets { get; set; } = new List<Ticket>();    
}