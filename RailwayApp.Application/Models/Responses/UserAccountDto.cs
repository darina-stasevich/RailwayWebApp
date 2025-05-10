using MongoDB.Bson.Serialization.Attributes;
using RailwayApp.Domain.Statuses;

namespace RailwayApp.Application.Models;

public class UserAccountDto
{
    public string Email { get; set; }

    public string Surname { get; set; }
    public string Name { get; set; }
    public string? SecondName { get; set; }
    
    public string? PhoneNumber { get; set; }
    
    public DateTime BirthDate { get; set; }
    
    [BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public Gender? Gender { get; set; }
    
}