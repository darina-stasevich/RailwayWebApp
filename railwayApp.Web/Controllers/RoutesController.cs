using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using RailwayApp.Application.Models;
using RailwayApp.Domain.Interfaces.IServices;

namespace RailwayApp.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoutesController(IRouteSearchService routeSearchService, ILogger<RoutesController> logger) : ControllerBase
{
    [HttpPost("search")]
    [ProducesResponseType(typeof(List<ComplexRouteDto>), 200)]
    [ProducesResponseType(typeof(ValidationProblemDetails), 400)] // Уточненный тип для ошибок валидации модели
    [ProducesResponseType(500)]
    public async Task<ActionResult<List<ComplexRouteDto>>> SearchRoutes([FromBody] RouteSearchRequest searchRequest)
    {
        string requestJson = "Error serializing request"; // Значение по умолчанию
        try
        {
            requestJson = JsonSerializer.Serialize(searchRequest);
        }
        catch (Exception ex) {
            logger.LogError(ex, "Failed to serialize RouteRequest for logging.");
        }

        logger.LogInformation("Received route search request: {RouteRequest}", requestJson);
        
        if (searchRequest.FromStationId == searchRequest.ToStationId)
        {
            ModelState.AddModelError(nameof(searchRequest.ToStationId), "from station id and to station id cannot be the same");
            logger.LogWarning("Validation failed for route search request {requestJson}", requestJson);
            return ValidationProblem(ModelState);
        }
        if (!ModelState.IsValid)
        {
            logger.LogWarning("Model validation failed for route search request: {ModelStateErrors}. Request: {RouteRequest}", JsonSerializer.Serialize(ModelState), requestJson);
            return ValidationProblem(ModelState);
        }   
        
        try
        {
            logger.LogInformation("Calling RouteSearchService.GetRoutesAsync for FromStationId: {FromStationId}, ToStationId: {ToStationId}, DepartureDate: {DepartureDate}, IsDirect: {IsDirect}",
                searchRequest.FromStationId, searchRequest.ToStationId, searchRequest.DepartureDate.ToString("yyyy-MM-dd"), searchRequest.IsDirectRoute);
            
            var routes = await routeSearchService.GetRoutesAsync(searchRequest);
            
            logger.LogInformation("Successfully retrieved {RouteCount} routes for request: {RouteRequest}", routes.Count, requestJson);
            return Ok(routes);
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning(ex, "ArgumentException caught during route search for request: {RouteRequest}", requestJson);
            return BadRequest();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception occurred during route search for request: {RouteRequest}. {Message}", requestJson, ex.Message);
            return StatusCode(500, "Internal Server Error");
        }
    }
}