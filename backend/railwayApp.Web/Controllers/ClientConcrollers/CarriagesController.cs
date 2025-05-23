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
public class CarriagesController : ControllerBase
{
    private readonly ICarriageService _carriageService;
    private readonly ILogger<CarriagesController> _logger; // Стандартный логгер (Serilog)
    private readonly IMyCustomLogger _customLogger;       // Твой кастомный логгер
    private const string LoggerNameForCustomLog = "CarriagesController"; // Имя для кастомного логгера

    public CarriagesController(
        ICarriageService carriageService,
        ILogger<CarriagesController> logger,
        IMyCustomLogger customLogger) // Инъекция твоего кастомного логгера
    {
        _carriageService = carriageService;
        _logger = logger;
        _customLogger = customLogger; // Сохраняем его
    }

    [HttpPost("summaries")]
    public async Task<ActionResult<List<ShortCarriageInfoDto>>> GetCarriageSummaries(
        [FromBody] CarriagesInfoRequest request)
    {
        if (!ModelState.IsValid)
        {
            // Стандартное логирование (Serilog)
            var requestJsonForSerilog = System.Text.Json.JsonSerializer.Serialize(request);
            _logger.LogWarning(
                "Model validation failed for carriage summaries request: {ModelStateErrors}. Request: {RouteRequest}",
                System.Text.Json.JsonSerializer.Serialize(ModelState), requestJsonForSerilog);

            // Твое кастомное логирование
            var modelStateErrors = ModelState.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
            );
            _customLogger.Warn("Model validation failed for carriage summaries request", LoggerNameForCustomLog,
                exception: null,
                context: new Dictionary<string, object>
                {
                    { "ModelStateErrors", modelStateErrors },
                    { "FailedRequestData", request } // Передаем сам объект request
                });
            return ValidationProblem(ModelState);
        }

        // Стандартное логирование (Serilog)
        _logger.LogInformation(
            "Getting carriage summaries for ConcreteRouteId: {ConcreteRouteId}, StartSegmentNumber: {StartSegmentNumber}, EndSegmentNumber: {EndSegmentNumber}",
            request.ConcreteRouteId, request.StartSegmentNumber, request.EndSegmentNumber);

        // Твое кастомное логирование
        _customLogger.Info("Getting carriage summaries", LoggerNameForCustomLog,
            context: new Dictionary<string, object>
            {
                { "ConcreteRouteId", request.ConcreteRouteId },
                { "StartSegmentNumber", request.StartSegmentNumber },
                { "EndSegmentNumber", request.EndSegmentNumber },
                { "FullRequest", request } // Можно передать весь объект запроса для полноты
            });

        var carriages = await _carriageService.GetAllCarriagesInfo(request);

        // Стандартное логирование (Serilog)
        _logger.LogInformation("Successfully retrieved {CarriageCount} carriage summaries for request: {CarriageRequest}",
            carriages.Count(), System.Text.Json.JsonSerializer.Serialize(request));

        // Твое кастомное логирование
        _customLogger.Info("Successfully retrieved carriage summaries", LoggerNameForCustomLog,
            context: new Dictionary<string, object>
            {
                { "CarriageSummaryCount", carriages.Count() },
                { "OriginalRequest", request }
                // Можно добавить сами summaries, если они не слишком большие и это полезно:
                // { "RetrievedSummaries", carriages }
            });
        return Ok(carriages);
    }

    [HttpPost("details")]
    public async Task<ActionResult<DetailedCarriageInfoDto>> GetCarriageDetails([FromBody] CarriageInfoRequest request)
    {
        if (!ModelState.IsValid)
        {
            // Стандартное логирование (Serilog)
            var requestJsonForSerilog = System.Text.Json.JsonSerializer.Serialize(request);
            _logger.LogWarning(
                "Model validation failed for carriage details request: {ModelStateErrors}. Request: {RouteRequest}",
                System.Text.Json.JsonSerializer.Serialize(ModelState), requestJsonForSerilog);

            // Твое кастомное логирование
            var modelStateErrors = ModelState.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
            );
            _customLogger.Warn("Model validation failed for carriage details request", LoggerNameForCustomLog,
                exception: null,
                context: new Dictionary<string, object>
                {
                    { "ModelStateErrors", modelStateErrors },
                    { "FailedRequestData", request } // Передаем сам объект request
                });
            return ValidationProblem(ModelState);
        }

        // Стандартное логирование (Serilog)
        _logger.LogInformation(
            "Getting carriage details for ConcreteRouteId: {ConcreteRouteId}, StartSegmentNumber: {StartSegmentNumber}, EndSegmentNumber: {EndSegmentNumber}, CarriageNumber: {CarriageNumber}",
            request.ConcreteRouteId, request.StartSegmentNumber, request.EndSegmentNumber, request.CarriageNumber);

        // Твое кастомное логирование
        _customLogger.Info("Getting carriage details", LoggerNameForCustomLog,
            context: new Dictionary<string, object>
            {
                { "ConcreteRouteId", request.ConcreteRouteId },
                { "StartSegmentNumber", request.StartSegmentNumber },
                { "EndSegmentNumber", request.EndSegmentNumber },
                { "CarriageNumber", request.CarriageNumber },
                { "FullRequest", request } // Можно передать весь объект запроса для полноты
            });

        var carriage = await _carriageService.GetCarriageInfo(request);

        if (carriage == null)
        {
            // Стандартное логирование (Serilog)
            _logger.LogWarning("Carriage details not found for request: {CarriageRequest}", System.Text.Json.JsonSerializer.Serialize(request));
            // Твое кастомное логирование
            _customLogger.Warn("Carriage details not found", LoggerNameForCustomLog,
                exception: null,
                context: new Dictionary<string, object> { { "Request", request } });
            return NotFound("Carriage details not found.");
        }

        // Стандартное логирование (Serilog)
        _logger.LogInformation("Successfully retrieved carriage details for request: {CarriageRequest}",
            System.Text.Json.JsonSerializer.Serialize(request));

        // Твое кастомное логирование
        _customLogger.Info("Successfully retrieved carriage details", LoggerNameForCustomLog,
            context: new Dictionary<string, object>
            {
                { "OriginalRequest", request },
                // Можно добавить сами детали, если они не слишком большие и это полезно:
                // { "RetrievedCarriageDetails", carriage }
            });
        return Ok(carriage);
    }
}