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
        return (await stationRepository.GetAllAsync()).ToList();
    }

    public async Task<Guid> CreateStationAsync(CreateStationRequest request)
    {
        var existingStation = await stationRepository.GetByNameAsync(request.Name);

        if (existingStation != null)
        {
            throw new InvalidOperationException($"Station {request.Region} already exists");
        }
        
        var station = new Station 
        { 
            Name = request.Name,
            Region = request.Region
        };

        var id = await stationRepository.AddAsync(station);
        return id;
    }
}