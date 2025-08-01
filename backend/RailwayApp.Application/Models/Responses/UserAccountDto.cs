using MongoDB.Bson;
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
    public DateOnly BirthDate { get; set; }
    [BsonRepresentation(BsonType.String)] public Gender? Gender { get; set; }
}