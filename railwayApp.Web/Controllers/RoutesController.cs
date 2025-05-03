using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using RailwayApp.Application.Models;
using RailwayApp.Domain.Interfaces.IServices;

namespace railway_service.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoutesController(IRouteSearchService routeSearchService, ILogger<RoutesController> logger) : ControllerBase
{
    [HttpPost("search")]
    [ProducesResponseType(typeof(List<ComplexRouteDto>), 200)]
    [ProducesResponseType(typeof(ValidationProblemDetails), 400)] // Уточненный тип для ошибок валидации модели
    [ProducesResponseType(500)]
    public async Task<ActionResult<List<ComplexRouteDto>>> SearchRoutes([FromBody] RouteRequest request)
    {
        string requestJson = "Error serializing request"; // Значение по умолчанию
        try
        {
            requestJson = JsonSerializer.Serialize(request);
        }
        catch (Exception ex) {
            logger.LogError(ex, "Failed to serialize RouteRequest for logging.");
        }

        logger.LogInformation("Received route search request: {RouteRequest}", requestJson);
        
        if (request.FromStationId == request.ToStationId)
        {
            ModelState.AddModelError(nameof(request.ToStationId), "from station id and to station id cannot be the same");
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
                request.FromStationId, request.ToStationId, request.DepartureDate?.ToString("yyyy-MM-dd"), request.IsDirect);
            
            var routes = await routeSearchService.GetRoutesAsync(
                request.FromStationId!.Value, 
                request.ToStationId!.Value,      
                request.DepartureDate!.Value.Date, 
                request.IsDirect
            );
            
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
            logger.LogError(ex, "Unhandled exception occurred during route search for request: {RouteRequest}", requestJson);
            return StatusCode(500, "Internal Server Error");
        }
    }
}