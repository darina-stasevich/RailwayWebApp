using System.ComponentModel.DataAnnotations;

namespace RailwayApp.Application.Models;

public class BookSeatRequest
{
    [Required(ErrorMessage = "id of route is required")]
    public Guid ConcreteRouteId { get; set; }
    
    [Required(ErrorMessage = "Start segment number is required")]
    public int StartSegmentNumber { get; set; }
    
    [Required(ErrorMessage = "End segment number is required")]
    public int EndSegmentNumber { get; set; }
    
    [Required(ErrorMessage = "carriage template id is required")]
    public int CarriageNumber { get; set; }
    
    [Required(ErrorMessage = "seat number is required")]
    public int SeatNumber { get; set; }
}