using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using RailwayApp.Domain.Statuses;

namespace RailwayApp.Domain.Entities;

public class PassengerData
{
    [Required(ErrorMessage = "Surname required")]
    [MinLength(2, ErrorMessage = "Minimal surname's length is 2")]
    [MaxLength(40, ErrorMessage = "Maximal surname's length is 40")]
    public string Surname { get; set; }

    [Required(ErrorMessage = "Name required")]
    [MinLength(2, ErrorMessage = "Minimal name's length is 2")]
    [MaxLength(40, ErrorMessage = "Maximal name's length is 40")]
    public string FirstName { get; set; }

    [MinLength(2, ErrorMessage = "Minimal second name's length is 2")]
    [MaxLength(40, ErrorMessage = "Maximal second name's length is 40")]
    public string? SecondName { get; set; }

    [BsonRepresentation(BsonType.String)] public Gender Gender { get; set; }

    public DateOnly BirthDate { get; set; }

    [RegularExpression(
        @"^[A-Z]{2}\d{7}$",
        ErrorMessage = "Incorrect format. Expected format is +AA0000000."
    )]
    [StringLength(9)]
    public string PassportNumber { get; set; }
}