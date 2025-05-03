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
        try
        {
            var stations = await stationRepository.GetAllAsync();
            return stations;
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public async Task<Guid> CreateStationAsync(CreateStationRequest request)
    {
        Station? existingStation = null;
        try
        {
            existingStation = await stationRepository.GetByNameAsync(request.Name);
        }
        catch (Exception ex)
        {
            throw;
        }

        if (existingStation != null)
        {
            throw new InvalidOperationException($"Station {request.Name} already exists");
        }
        
        var station = new Station 
        { 
            Name = request.Name,
            Region = request.Region
        };

        try
        {
            var id = await stationRepository.CreateAsync(station);
            return id;
        }
        catch (Exception ex)
        {
            throw;
        }
    }
}