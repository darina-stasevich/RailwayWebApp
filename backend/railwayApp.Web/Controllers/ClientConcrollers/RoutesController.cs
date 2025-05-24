
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using railway_service;
using RailwayApp.Application.Models; 
using RailwayApp.Domain.Interfaces.IServices;

namespace RailwayApp.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Client")]
public class RoutesController(
    IRouteSearchService routeSearchService,
    ILogger<RoutesController> logger,
    IMyCustomLogger customLogger)
    : ControllerBase
{
    private const string LoggerNameForCustomLog = "RoutesController"; 
    
    [HttpPost("search")]
    public async Task<ActionResult<List<ComplexRouteDto>>> SearchRoutes([FromBody] RouteSearchRequest searchRequest)
    {
        var requestJsonForSerilog = System.Text.Json.JsonSerializer.Serialize(searchRequest);

        logger.LogInformation("Received route search request: {RouteRequest}", requestJsonForSerilog);

        customLogger.Info("Received route search request", LoggerNameForCustomLog,
            context: new Dictionary<string, object>
            {
                { "SearchRequest", searchRequest }
            });

        if (searchRequest.FromStationId == searchRequest.ToStationId)
        {
            ModelState.AddModelError(nameof(searchRequest.ToStationId),
                "From station ID and To station ID cannot be the same.");

            logger.LogWarning("Validation failed: FromStationId and ToStationId are the same. Request: {requestJson}", requestJsonForSerilog);

            customLogger.Warn("Validation failed: FromStationId and ToStationId are the same", LoggerNameForCustomLog,
                exception: null,
                context: new Dictionary<string, object>
                {
                    { "FromStationId", searchRequest.FromStationId },
                    { "ToStationId", searchRequest.ToStationId },
                    { "FullRequest", searchRequest }
                });
            return ValidationProblem(ModelState);
        }

        if (!ModelState.IsValid)
        {
            logger.LogWarning(
                "Model validation failed for route search request: {ModelStateErrors}. Request: {RouteRequest}",
                System.Text.Json.JsonSerializer.Serialize(ModelState), requestJsonForSerilog);

            var modelStateErrors = ModelState.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
            );
            customLogger.Warn("Model validation failed for route search request", LoggerNameForCustomLog,
                exception: null,
                context: new Dictionary<string, object>
                {
                    { "ModelStateErrors", modelStateErrors },
                    { "FailedRequestData", searchRequest }
                });
            return ValidationProblem(ModelState);
        }

        logger.LogInformation(
            "Calling RouteSearchService.GetRoutesAsync for FromStationId: {FromStationId}, ToStationId: {ToStationId}, DepartureDate: {DepartureDate}, IsDirect: {IsDirect}",
            searchRequest.FromStationId, searchRequest.ToStationId, searchRequest.DepartureDate.ToString("yyyy-MM-dd"),
            searchRequest.IsDirectRoute);

        customLogger.Info("Calling RouteSearchService.GetRoutesAsync", LoggerNameForCustomLog,
            context: new Dictionary<string, object>
            {
                { "FromStationId", searchRequest.FromStationId },
                { "ToStationId", searchRequest.ToStationId },
                { "DepartureDate", searchRequest.DepartureDate }, 
                { "IsDirectRoute", searchRequest.IsDirectRoute },
                { "FullRequest", searchRequest }
            });

        var routes = await routeSearchService.GetRoutesAsync(searchRequest);

        logger.LogInformation("Successfully retrieved {RouteCount} routes for request: {RouteRequest}", routes.Count(),
            requestJsonForSerilog);

        customLogger.Info("Successfully retrieved routes", LoggerNameForCustomLog,
            context: new Dictionary<string, object>
            {
                { "RouteCount", routes.Count() },
                { "OriginalRequest", searchRequest }
                 });
        return Ok(routes);
    }
}