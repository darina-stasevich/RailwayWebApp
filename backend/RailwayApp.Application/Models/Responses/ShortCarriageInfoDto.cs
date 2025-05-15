namespace RailwayApp.Application.Models;

public class ShortCarriageInfoDto
{
    public int CarriageNumber { get; set; }
    public string LayoutIdentifier { get; set; }
    public int AvailableSeats { get; set; }
    public decimal Cost { get; set; }
}