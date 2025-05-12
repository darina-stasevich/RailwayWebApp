using RailwayApp.Application.Models;

namespace RailwayApp.Domain.Interfaces.IServices;

public interface IRouteSearchService
{
    Task<IEnumerable<ComplexRouteDto>> GetRoutesAsync(RouteSearchRequest request);
}