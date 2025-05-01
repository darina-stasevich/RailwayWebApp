using MongoDB.Bson.Serialization.Attributes;
using RailwayApp.Domain.Statuses;

namespace RailwayApp.Domain.Entities;

// поставить email как index,
// разобраться с Id

public class UserAccount
{
    [BsonId]
    public string Email { get; set; } // key
    
    public string Surname { get; set; }
    public string Name { get; set; }
    public string? SecondName { get; set; }
    
    public string? PhoneNumber { get; set; }
    
    public DateTime BirthDate { get; set; }
    
    public string? PassportNumber { get; set; }
    public Gender? Gender { get; set; }
    
    public string HashedPassword { get; set; }
    public UserAccountStatus Status { get; set; }
    
    // public List<Ticket> Tickets { get; set; } = new List<Ticket>();    
}