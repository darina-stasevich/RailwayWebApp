using System.ComponentModel.DataAnnotations;

namespace RailwayApp.Application.Models;

public class CarriagesInfoRequest
{
    [Required]
    public Guid ConcreteRouteId { get; set; }
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Start segment number must be greater than 0.")]
    public int StartSegmentNumber { get; set; }
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "End segment number must be greater than 1.")]
    public int EndSegmentNumber { get; set; }
}