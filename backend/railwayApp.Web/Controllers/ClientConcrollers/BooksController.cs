using System.Security.Claims;
using System.Text.Json; // Для стандартного JsonSerializer, если он нужен для других целей
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using railway_service;
using RailwayApp.Application.Models; // Для BookSeatRequest, SeatLockResponse
using RailwayApp.Domain.Interfaces.IServices; // Для ITicketBookingService
// Предполагается, что IMyCustomLogger и его реализация (MyCustomFileJsonLogger) определены
// и зарегистрированы в DI.
// Например, в Program.cs:
// builder.Services.AddSingleton<IMyCustomLogger, MyCustomFileJsonLogger>();

namespace RailwayApp.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Client")]
public class BooksController : ControllerBase
{
    private readonly ITicketBookingService _ticketBookingService;
    private readonly ILogger<BooksController> _logger; // Стандартный логгер (Serilog)
    private readonly IMyCustomLogger _customLogger;   // Твой кастомный логгер
    private const string LoggerNameForCustomLog = "BooksController"; // Имя для кастомного логгера

    public BooksController(
        ITicketBookingService ticketBookingService,
        ILogger<BooksController> logger,
        IMyCustomLogger customLogger) // Инъекция твоего кастомного логгера
    {
        _ticketBookingService = ticketBookingService;
        _logger = logger;
        _customLogger = customLogger; // Сохраняем его
    }

    [HttpPost("book-seats")]
    public async Task<ActionResult<Guid>> BookSeats([FromBody] List<BookSeatRequest> requests)
    {
        if (!ModelState.IsValid)
        {
            // Стандартное логирование (Serilog)
            // Обрати внимание: JsonSerializer.Serialize(requests) может быть тяжелой операцией,
            // особенно если список большой. Serilog с оператором @ сделал бы это эффективнее.
            var requestJsonForSerilog = System.Text.Json.JsonSerializer.Serialize(requests);
            _logger.LogWarning(
                "Model validation failed for book seats request: {ModelStateErrors}. Request: {RouteRequest}",
                System.Text.Json.JsonSerializer.Serialize(ModelState), requestJsonForSerilog);

            // Твое кастомное логирование
            // Для ModelState можно собрать ошибки вручную или сериализовать его целиком как объект
            var modelStateErrors = ModelState.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
            );
            _customLogger.Warn("Model validation failed for book seats request", LoggerNameForCustomLog,
                exception: null, // Здесь нет прямого исключения, только ошибки модели
                context: new Dictionary<string, object>
                {
                    { "ModelStateErrors", modelStateErrors },
                    // Передаем сам список объектов requests.
                    // Твой MyCustomJsonSerializer должен превратить его в JSON-массив объектов.
                    { "FailedRequestData", requests }
                });
            return ValidationProblem(ModelState);
        }

        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userAccountId))
        {
            // Стандартное логирование (Serilog)
            _logger.LogWarning("User ID claim is missing or invalid in BookSeats request. Path: {Path}", HttpContext.Request.Path);

            // Твое кастомное логирование
            _customLogger.Warn("User ID claim is missing or invalid in BookSeats request", LoggerNameForCustomLog,
                exception: null,
                context: new Dictionary<string, object>
                {
                    { "Path", HttpContext.Request.Path.ToString() }
                });
            return Unauthorized("User ID claim is missing or invalid.");
        }

        // Стандартное логирование (Serilog)
        // Опять же, сериализация всего списка запросов может быть затратной.
        var requestJsonForSerilogInfo = System.Text.Json.JsonSerializer.Serialize(requests);
        _logger.LogInformation("Starting to book seats for User {UserAccountId}. Request: {requests}", userAccountId, requestJsonForSerilogInfo);

        // Твое кастомное логирование
        _customLogger.Info("Starting to book seats", LoggerNameForCustomLog,
            context: new Dictionary<string, object>
            {
                { "UserAccountId", userAccountId },
                // Передаем сам список объектов requests.
                { "BookSeatRequests", requests }
            });

        var result = await _ticketBookingService.BookPlaces(userAccountId, requests);

        // Стандартное логирование (Serilog)
        _logger.LogInformation("Successfully booked seats for User {UserAccountId}, got SeatLockId {SeatLockId}. OriginalRequest: {requests}",
            userAccountId, result, requestJsonForSerilogInfo);

        // Твое кастомное логирование
        _customLogger.Info("Successfully booked seats", LoggerNameForCustomLog,
            context: new Dictionary<string, object>
            {
                { "UserAccountId", userAccountId },
                { "SeatLockId", result },
                // Можно снова залогировать запросы, если это нужно для отладки,
                // но возможно, достаточно SeatLockId и информации из предыдущего лога.
                { "OriginalRequests", requests }
            });

        return Ok(result);
    }

    [HttpPost("cancel-books")] // Изменил cancelBooks на cancel-books для единообразия с kebab-case
    public async Task<ActionResult<bool>> CancelBookSeats([FromBody] Guid seatLockId) // Принимаем Guid из тела запроса
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userAccountId))
        {
            // Стандартное логирование (Serilog)
            _logger.LogWarning("User ID claim is missing or invalid in CancelBookSeats request for SeatLockId {SeatLockId}. Path: {Path}", seatLockId, HttpContext.Request.Path);

            // Твое кастомное логирование
            _customLogger.Warn("User ID claim is missing or invalid in CancelBookSeats request", LoggerNameForCustomLog,
                exception: null,
                context: new Dictionary<string, object>
                {
                    { "SeatLockId", seatLockId },
                    { "Path", HttpContext.Request.Path.ToString() }
                });
            return Unauthorized("User ID claim is missing or invalid.");
        }

        // Стандартное логирование (Serilog)
        _logger.LogInformation("Attempting to cancel seat lock {SeatLockId} for User {UserAccountId}", seatLockId, userAccountId);

        // Твое кастомное логирование
        _customLogger.Info("Attempting to cancel seat lock", LoggerNameForCustomLog,
            context: new Dictionary<string, object>
            {
                { "UserAccountId", userAccountId },
                { "SeatLockId", seatLockId }
            });

        var result = await _ticketBookingService.CancelBookPlaces(userAccountId, seatLockId);

        if (result)
        {
            // Стандартное логирование (Serilog)
            _logger.LogInformation("Successfully canceled seat lock {SeatLockId} for User {UserAccountId}", seatLockId, userAccountId);
            // Твое кастомное логирование
            _customLogger.Info("Successfully canceled seat lock", LoggerNameForCustomLog,
                context: new Dictionary<string, object>
                {
                    { "UserAccountId", userAccountId },
                    { "SeatLockId", seatLockId },
                    { "CancellationResult", result }
                });
        }
        else
        {
            // Стандартное логирование (Serilog) - возможно, стоит использовать LogWarning, если отмена не удалась по бизнес-логике
            _logger.LogWarning("Failed to cancel seat lock {SeatLockId} for User {UserAccountId}. Service returned {ServiceResult}", seatLockId, userAccountId, result);
            // Твое кастомное логирование
            _customLogger.Warn("Failed to cancel seat lock", LoggerNameForCustomLog, exception: null,
                context: new Dictionary<string, object>
                {
                    { "UserAccountId", userAccountId },
                    { "SeatLockId", seatLockId },
                    { "CancellationResult", result }
                });
        }
        return Ok(result);
    }

    [HttpGet("my-bookings")]
    public async Task<ActionResult<List<SeatLockResponse>>> GetMyBookings() // Изменил GetSeatLocks на GetMyBookings для единообразия
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userAccountId))
        {
            // Стандартное логирование (Serilog)
            _logger.LogWarning("User ID claim is missing or invalid in GetMyBookings request. Path: {Path}", HttpContext.Request.Path);
            // Твое кастомное логирование
            _customLogger.Warn("User ID claim is missing or invalid in GetMyBookings request", LoggerNameForCustomLog,
                exception: null,
                context: new Dictionary<string, object>
                {
                    { "Path", HttpContext.Request.Path.ToString() }
                });
            return Unauthorized("User ID claim is missing or invalid");
        }

        // Стандартное логирование (Serilog)
        _logger.LogInformation("Attempting to get seat locks for User {UserAccountId}", userAccountId);

        // Твое кастомное логирование
        _customLogger.Info("Attempting to get seat locks for user", LoggerNameForCustomLog,
            context: new Dictionary<string, object>
            {
                { "UserAccountId", userAccountId }
            });

        var result = await _ticketBookingService.GetBooks(userAccountId);

        // Стандартное логирование (Serilog)
        _logger.LogInformation("Successfully retrieved {SeatLockCount} seat locks for User {UserAccountId}", result.Count(), userAccountId);

        // Твое кастомное логирование
        _customLogger.Info("Successfully retrieved seat locks for user", LoggerNameForCustomLog,
            context: new Dictionary<string, object>
            {
                { "UserAccountId", userAccountId },
                { "SeatLockCount", result.Count() },
                // Можно добавить сами SeatLocks, если они не слишком большие и это полезно для лога
                // { "RetrievedSeatLocks", result } // Раскомментируй, если нужно
            });
        return Ok(result.ToList()); // Убедимся, что возвращаем List, как и заявлено
    }
}