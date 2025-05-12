using RailwayApp.Application.Models;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces.IRepositories;

namespace RailwayApp.Domain.Interfaces.IServices;

public interface IStationService
{
    Task<IEnumerable<Station>> GetAllStationsAsync();
    Task<Guid> CreateStationAsync(CreateStationRequest request);
}