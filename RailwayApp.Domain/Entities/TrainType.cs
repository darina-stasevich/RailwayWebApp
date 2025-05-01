namespace RailwayApp.Domain.Entities;

// name is unique identifier
public class TrainType
{
    public string Name { get; set; }
    public List<CarriageTemplate> CarriageTemplates { get; set; } = new();
}