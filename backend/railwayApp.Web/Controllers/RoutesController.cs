using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RailwayApp.Application.Models;
using RailwayApp.Domain.Interfaces.IServices;

namespace RailwayApp.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Client")]
public class RoutesController(IRouteSearchService routeSearchService, ILogger<RoutesController> logger) : ControllerBase
{
    [HttpPost("search")]
    public async Task<ActionResult<List<ComplexRouteDto>>> SearchRoutes([FromBody] RouteSearchRequest searchRequest)
    {
        var requestJson = JsonSerializer.Serialize(searchRequest);

        logger.LogInformation("Received route search request: {RouteRequest}", requestJson);

        if (searchRequest.FromStationId == searchRequest.ToStationId)
        {
            ModelState.AddModelError(nameof(searchRequest.ToStationId),
                "from station id and to station id cannot be the same");
            logger.LogWarning("Validation failed for route search request {requestJson}", requestJson);
            return ValidationProblem(ModelState);
        }

        if (!ModelState.IsValid)
        {
            logger.LogWarning(
                "Model validation failed for route search request: {ModelStateErrors}. Request: {RouteRequest}",
                JsonSerializer.Serialize(ModelState), requestJson);
            return ValidationProblem(ModelState);
        }

        logger.LogInformation(
            "Calling RouteSearchService.GetRoutesAsync for FromStationId: {FromStationId}, ToStationId: {ToStationId}, DepartureDate: {DepartureDate}, IsDirect: {IsDirect}",
            searchRequest.FromStationId, searchRequest.ToStationId, searchRequest.DepartureDate.ToString("yyyy-MM-dd"),
            searchRequest.IsDirectRoute);

        var routes = await routeSearchService.GetRoutesAsync(searchRequest);

        logger.LogInformation("Successfully retrieved {RouteCount} routes for request: {RouteRequest}", routes.Count(),
            requestJson);
        return Ok(routes);
    }
}