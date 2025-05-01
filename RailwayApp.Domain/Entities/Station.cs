namespace RailwayApp.Domain.Entities;

    
public class Station
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Region { get; set; }
    
    public List<AbstractRouteSegment> DepartureSegments { get; set; } = new(); 
    public List<AbstractRouteSegment> ArrivalSegments { get; set; } = new();
}