using System.ComponentModel.DataAnnotations;

namespace RailwayApp.Application.Models;

public class CarriageInfoRequest
{
    [Required] public Guid ConcreteRouteId { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Start segment number must be greater than 0.")]
    public int StartSegmentNumber { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "End segment number must be greater than 1.")]
    public int EndSegmentNumber { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Carriage number must be greater than 0.")]
    public int CarriageNumber { get; set; }
}