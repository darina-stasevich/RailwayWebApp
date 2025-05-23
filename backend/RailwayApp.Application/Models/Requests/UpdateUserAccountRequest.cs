using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using RailwayApp.Domain.Statuses;

namespace RailwayApp.Application.Models;

public class UpdateUserAccountRequest
{
    [Required(ErrorMessage = "Surname required")]
    [MinLength(2, ErrorMessage = "Minimal surname's length is 2")]
    [MaxLength(40, ErrorMessage = "Maximal surname's length is 40")]
    public string Surname { get; set; }

    [Required(ErrorMessage = "Name required")]
    [MinLength(2, ErrorMessage = "Minimal Name's length is 2")]
    [MaxLength(40, ErrorMessage = "Maximal Name's length is 40")]
    public string Name { get; set; }

    [MinLength(2, ErrorMessage = "Minimal Name's length is 2")]
    [MaxLength(40, ErrorMessage = "Maximal Name's length is 40")]
    public string? SecondName { get; set; }

    [RegularExpression(
        @"^\+375\s?\(?\d{2}\)?\s?\d{7}$", // Само регулярное выражение
        ErrorMessage = "Incorrect format. Expected format is +375(XX)XXXXXXX."
    )]
    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    [Required(ErrorMessage = "Birth day is required")]
    public DateOnly BirthDate { get; set; }

    [BsonRepresentation(BsonType.String)] public Gender? Gender { get; set; }
}