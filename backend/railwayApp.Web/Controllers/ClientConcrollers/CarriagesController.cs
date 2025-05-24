using System.Text.Json; // Для стандартного JsonSerializer, если он нужен для других целей
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using railway_service;
using RailwayApp.Application.Models; // Для CarriagesInfoRequest, ShortCarriageInfoDto, DetailedCarriageInfoDto, CarriageInfoRequest
using RailwayApp.Domain.Interfaces.IServices; // Для ICarriageService
// Предполагается, что IMyCustomLogger и его реализация (MyCustomFileJsonLogger) определены
// и зарегистрированы в DI.
// Например, в Program.cs:
// builder.Services.AddSingleton<IMyCustomLogger, MyCustomFileJsonLogger>();

namespace RailwayApp.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Client")]
public class CarriagesController(
    ICarriageService carriageService,
    ILogger<CarriagesController> logger,
    IMyCustomLogger customLogger)
    : ControllerBase
{
    private const string LoggerNameForCustomLog = "CarriagesController";

    [HttpPost("summaries")]
    public async Task<ActionResult<List<ShortCarriageInfoDto>>> GetCarriageSummaries(
        [FromBody] CarriagesInfoRequest request)
    {
        if (!ModelState.IsValid)
        {
            var requestJsonForSerilog = System.Text.Json.JsonSerializer.Serialize(request);
            logger.LogWarning(
                "Model validation failed for carriage summaries request: {ModelStateErrors}. Request: {RouteRequest}",
                System.Text.Json.JsonSerializer.Serialize(ModelState), requestJsonForSerilog);

            var modelStateErrors = ModelState.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
            );
            customLogger.Warn("Model validation failed for carriage summaries request", LoggerNameForCustomLog,
                exception: null,
                context: new Dictionary<string, object>
                {
                    { "ModelStateErrors", modelStateErrors },
                    { "FailedRequestData", request } 
                });
            return ValidationProblem(ModelState);
        }

        logger.LogInformation(
            "Getting carriage summaries for ConcreteRouteId: {ConcreteRouteId}, StartSegmentNumber: {StartSegmentNumber}, EndSegmentNumber: {EndSegmentNumber}",
            request.ConcreteRouteId, request.StartSegmentNumber, request.EndSegmentNumber);

        customLogger.Info("Getting carriage summaries", LoggerNameForCustomLog,
            context: new Dictionary<string, object>
            {
                { "ConcreteRouteId", request.ConcreteRouteId },
                { "StartSegmentNumber", request.StartSegmentNumber },
                { "EndSegmentNumber", request.EndSegmentNumber },
                { "FullRequest", request } 
            });

        var carriages = await carriageService.GetAllCarriagesInfo(request);

        logger.LogInformation("Successfully retrieved {CarriageCount} carriage summaries for request: {CarriageRequest}",
            carriages.Count(), System.Text.Json.JsonSerializer.Serialize(request));

        customLogger.Info("Successfully retrieved carriage summaries", LoggerNameForCustomLog,
            context: new Dictionary<string, object>
            {
                { "CarriageSummaryCount", carriages.Count() },
                { "OriginalRequest", request }
            });
        return Ok(carriages);
    }

    [HttpPost("details")]
    public async Task<ActionResult<DetailedCarriageInfoDto>> GetCarriageDetails([FromBody] CarriageInfoRequest request)
    {
        if (!ModelState.IsValid)
        {
            var requestJsonForSerilog = System.Text.Json.JsonSerializer.Serialize(request);
            logger.LogWarning(
                "Model validation failed for carriage details request: {ModelStateErrors}. Request: {RouteRequest}",
                System.Text.Json.JsonSerializer.Serialize(ModelState), requestJsonForSerilog);

            var modelStateErrors = ModelState.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
            );
            customLogger.Warn("Model validation failed for carriage details request", LoggerNameForCustomLog,
                exception: null,
                context: new Dictionary<string, object>
                {
                    { "ModelStateErrors", modelStateErrors },
                    { "FailedRequestData", request } 
                });
            return ValidationProblem(ModelState);
        }

        logger.LogInformation(
            "Getting carriage details for ConcreteRouteId: {ConcreteRouteId}, StartSegmentNumber: {StartSegmentNumber}, EndSegmentNumber: {EndSegmentNumber}, CarriageNumber: {CarriageNumber}",
            request.ConcreteRouteId, request.StartSegmentNumber, request.EndSegmentNumber, request.CarriageNumber);

        customLogger.Info("Getting carriage details", LoggerNameForCustomLog,
            context: new Dictionary<string, object>
            {
                { "ConcreteRouteId", request.ConcreteRouteId },
                { "StartSegmentNumber", request.StartSegmentNumber },
                { "EndSegmentNumber", request.EndSegmentNumber },
                { "CarriageNumber", request.CarriageNumber },
                { "FullRequest", request } 
            });

        var carriage = await carriageService.GetCarriageInfo(request);

        if (carriage == null)
        {
            logger.LogWarning("Carriage details not found for request: {CarriageRequest}", System.Text.Json.JsonSerializer.Serialize(request));
            customLogger.Warn("Carriage details not found", LoggerNameForCustomLog,
                exception: null,
                context: new Dictionary<string, object> { { "Request", request } });
            return NotFound("Carriage details not found.");
        }

        logger.LogInformation("Successfully retrieved carriage details for request: {CarriageRequest}",
            System.Text.Json.JsonSerializer.Serialize(request));

        customLogger.Info("Successfully retrieved carriage details", LoggerNameForCustomLog,
            context: new Dictionary<string, object>
            {
                { "OriginalRequest", request },
            });
        return Ok(carriage);
    }
}