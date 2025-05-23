using System.Text.Json; // Для стандартного JsonSerializer, если он нужен для других целей
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using railway_service;
using RailwayApp.Application.Models; // Для RouteSearchRequest, ComplexRouteDto
using RailwayApp.Domain.Interfaces.IServices; // Для IRouteSearchService
// Предполагается, что IMyCustomLogger и его реализация (MyCustomFileJsonLogger) определены
// и зарегистрированы в DI.
// Например, в Program.cs:
// builder.Services.AddSingleton<IMyCustomLogger, MyCustomFileJsonLogger>();

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
    private const string LoggerNameForCustomLog = "RoutesController"; // Имя для кастомного логгера
    
    [HttpPost("search")]
    public async Task<ActionResult<List<ComplexRouteDto>>> SearchRoutes([FromBody] RouteSearchRequest searchRequest)
    {
        // Для Serilog можно сериализовать заранее, если не используется @ оператор
        var requestJsonForSerilog = System.Text.Json.JsonSerializer.Serialize(searchRequest);

        // Стандартное логирование (Serilog)
        logger.LogInformation("Received route search request: {RouteRequest}", requestJsonForSerilog);

        // Твое кастомное логирование
        customLogger.Info("Received route search request", LoggerNameForCustomLog,
            context: new Dictionary<string, object>
            {
                { "SearchRequest", searchRequest } // Передаем весь объект
            });

        if (searchRequest.FromStationId == searchRequest.ToStationId)
        {
            ModelState.AddModelError(nameof(searchRequest.ToStationId),
                "From station ID and To station ID cannot be the same.");

            // Стандартное логирование (Serilog)
            logger.LogWarning("Validation failed: FromStationId and ToStationId are the same. Request: {requestJson}", requestJsonForSerilog);

            // Твое кастомное логирование
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
            // Стандартное логирование (Serilog)
            logger.LogWarning(
                "Model validation failed for route search request: {ModelStateErrors}. Request: {RouteRequest}",
                System.Text.Json.JsonSerializer.Serialize(ModelState), requestJsonForSerilog);

            // Твое кастомное логирование
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

        // Стандартное логирование (Serilog)
        logger.LogInformation(
            "Calling RouteSearchService.GetRoutesAsync for FromStationId: {FromStationId}, ToStationId: {ToStationId}, DepartureDate: {DepartureDate}, IsDirect: {IsDirect}",
            searchRequest.FromStationId, searchRequest.ToStationId, searchRequest.DepartureDate.ToString("yyyy-MM-dd"),
            searchRequest.IsDirectRoute);

        // Твое кастомное логирование
        customLogger.Info("Calling RouteSearchService.GetRoutesAsync", LoggerNameForCustomLog,
            context: new Dictionary<string, object>
            {
                { "FromStationId", searchRequest.FromStationId },
                { "ToStationId", searchRequest.ToStationId },
                { "DepartureDate", searchRequest.DepartureDate }, // TimeSpan будет сериализован твоим кастомным сериализатором
                { "IsDirectRoute", searchRequest.IsDirectRoute },
                { "FullRequest", searchRequest }
            });

        var routes = await routeSearchService.GetRoutesAsync(searchRequest);

        // Стандартное логирование (Serilog)
        logger.LogInformation("Successfully retrieved {RouteCount} routes for request: {RouteRequest}", routes.Count(),
            requestJsonForSerilog);

        // Твое кастомное логирование
        customLogger.Info("Successfully retrieved routes", LoggerNameForCustomLog,
            context: new Dictionary<string, object>
            {
                { "RouteCount", routes.Count() },
                { "OriginalRequest", searchRequest }
                // Можно добавить сами маршруты, если они не слишком большие и это полезно
                // { "RetrievedRoutes", routes }
            });
        return Ok(routes);
    }
}