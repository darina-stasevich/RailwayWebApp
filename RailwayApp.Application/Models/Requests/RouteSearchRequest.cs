using System.ComponentModel.DataAnnotations;

namespace RailwayApp.Application.Models;

public class RouteSearchRequest
{
    [Required(ErrorMessage = "from station id is required")]
    public Guid FromStationId { get; set; }

    [Required(ErrorMessage = "to station id is required")]
    public Guid ToStationId { get; set; }

    [Required(ErrorMessage = "departure date is required")]
    public DateTime DepartureDate { get; set; }
    public bool IsDirectRoute { get; set; } = true;
}