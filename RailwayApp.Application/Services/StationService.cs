using Microsoft.Extensions.Logging;
using RailwayApp.Application.Models;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces.IRepositories;
using RailwayApp.Domain.Interfaces.IServices;

namespace RailwayApp.Application.Services;

public class StationService(IStationRepository stationRepository) : IStationService
{
    public async Task<List<Station>> GetAllStationsAsync()
    {
        return await stationRepository.GetAllAsync();
    }

    public async Task<Guid> CreateStationAsync(string name, string region)
    {
        var existingStation = await stationRepository.GetByNameAsync(name);

        if (existingStation != null)
        {
            throw new InvalidOperationException($"Station {name} already exists");
        }
        
        var station = new Station 
        { 
            Name = name,
            Region = region
        };

        var id = await stationRepository.CreateAsync(station);
        return id;
    }
}