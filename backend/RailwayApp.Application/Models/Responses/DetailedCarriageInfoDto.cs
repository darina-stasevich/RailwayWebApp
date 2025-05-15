namespace RailwayApp.Application.Models;

public class DetailedCarriageInfoDto
{
    public int CarriageNumber { get; set; }
    public string LayoutIdentifier { get; set; }
    public List<int> AvailableSeats { get; set; } = new();

    public decimal Cost { get; set; }
    public int TotalSeats { get; set; }
}