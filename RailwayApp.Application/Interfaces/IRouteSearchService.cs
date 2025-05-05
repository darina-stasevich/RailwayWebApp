using RailwayApp.Application.Models;

namespace RailwayApp.Domain.Interfaces.IServices;

public interface IRouteSearchService
{
    Task<List<ComplexRouteDto>> GetRoutesAsync(RouteSearchRequest request);
}