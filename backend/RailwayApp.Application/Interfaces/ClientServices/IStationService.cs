using RailwayApp.Application.Models;
using RailwayApp.Domain.Entities;

namespace RailwayApp.Domain.Interfaces.IServices;

public interface IStationService
{
    Task<IEnumerable<Station>> GetAllStationsAsync();
}