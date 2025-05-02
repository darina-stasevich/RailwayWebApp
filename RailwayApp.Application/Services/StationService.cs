using Microsoft.Extensions.Logging;
using RailwayApp.Application.Models;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces.IRepositories;
using RailwayApp.Domain.Interfaces.IServices;

namespace RailwayApp.Application.Services;

public class StationService(ILogger<StationService> logger, IStationRepository stationRepository) : IStationService
{
    public async Task<List<Station>> GetAllStationsAsync()
    {
        var stations = await stationRepository.GetAllAsync();
        logger.LogInformation("GetAllStationsAsync get {stations.Count} stations", stations.Count);
        
        return stations;
    }

    public async Task<Guid> CreateStationAsync(CreateStationRequest request)
    {
        var existingStation = await stationRepository.GetByNameAsync(request.Name);
        if (existingStation != null)
        {
            logger.LogWarning("Station {Name} already exists", request.Name);
            throw new InvalidOperationException($"Station {request.Name} already exists");
        }
        
        // Создание новой станции
        var station = new Station 
        { 
            Name = request.Name,
            Region = request.Region
        };

        await stationRepository.CreateAsync(station);
        logger.LogInformation("Create station: {Id} - {Name} - {Region}", station.Id, station.Name, station.Region);

        return station.Id;
    }
}