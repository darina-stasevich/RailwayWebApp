using MongoDB.Bson.Serialization.Attributes;

namespace RailwayApp.Domain.Entities;

    
public class Station
{
    [BsonId]
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; }
    public string Region { get; set; }
    
    //public List<AbstractRouteSegment> DepartureSegments { get; set; } = new(); 
    //public List<AbstractRouteSegment> ArrivalSegments { get; set; } = new();
}