using RailwayApp.Application.Models;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces.IRepositories;
using RailwayApp.Domain.Interfaces.IServices;

namespace RailwayApp.Application.Services;

public class StationService(IStationRepository stationRepository) : IStationService
{
    public async Task<IEnumerable<Station>> GetAllStationsAsync()
    {
        return await stationRepository.GetAllAsync();
    }

}