using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using RailwayApp.Domain.Statuses;

namespace RailwayApp.Domain.Entities;

public class UserAccount : IEntity<Guid>
{
    [BsonId] public Guid Id { get; set; } = Guid.NewGuid();
    public string Email { get; set; }
    public string Surname { get; set; }
    public string Name { get; set; }
    public string? SecondName { get; set; }
    public string? PhoneNumber { get; set; }
    public DateOnly BirthDate { get; set; }

    [BsonRepresentation(BsonType.String)] public Gender? Gender { get; set; }

    public string HashedPassword { get; set; }

    [BsonRepresentation(BsonType.String)] public UserAccountStatus Status { get; set; }

    [BsonRepresentation(BsonType.String)] public UserRole Role { get; set; } = UserRole.Client;

    public DateTime StatusChangedDate { get; set; }
    
}