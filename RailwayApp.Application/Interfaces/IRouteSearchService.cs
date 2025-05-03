using RailwayApp.Application.Models;

namespace RailwayApp.Domain.Interfaces.IServices;

public interface IRouteSearchService
{
    Task<List<ComplexRouteDto>> GetRoutesAsync(Guid fromStationId, Guid toStationId, DateTime departureDate, bool isDirectRoute);
}