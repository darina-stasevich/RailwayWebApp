using System.Security.Claims;
using System.Text.Json; 
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using railway_service;
using RailwayApp.Application.Models; 
using RailwayApp.Domain.Interfaces.IServices; 
namespace RailwayApp.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Client")]
public class BooksController : ControllerBase
{
    private readonly ITicketBookingService _ticketBookingService;
    private readonly ILogger<BooksController> _logger; 
    private readonly IMyCustomLogger _customLogger;  
    private const string LoggerNameForCustomLog = "BooksController"; 

    public BooksController(
        ITicketBookingService ticketBookingService,
        ILogger<BooksController> logger,
        IMyCustomLogger customLogger) 
    {
        _ticketBookingService = ticketBookingService;
        _logger = logger;
        _customLogger = customLogger;
    }

    [HttpPost("book-seats")]
    public async Task<ActionResult<Guid>> BookSeats([FromBody] List<BookSeatRequest> requests)
    {
        if (!ModelState.IsValid)
        {
            var requestJsonForSerilog = System.Text.Json.JsonSerializer.Serialize(requests);
            _logger.LogWarning(
                "Model validation failed for book seats request: {ModelStateErrors}. Request: {RouteRequest}",
                System.Text.Json.JsonSerializer.Serialize(ModelState), requestJsonForSerilog);

            var modelStateErrors = ModelState.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
            );
            _customLogger.Warn("Model validation failed for book seats request", LoggerNameForCustomLog,
                exception: null,
                context: new Dictionary<string, object>
                {
                    { "ModelStateErrors", modelStateErrors },
                    { "FailedRequestData", requests }
                });
            return ValidationProblem(ModelState);
        }

        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userAccountId))
        {
            _logger.LogWarning("User ID claim is missing or invalid in BookSeats request. Path: {Path}", HttpContext.Request.Path);

            _customLogger.Warn("User ID claim is missing or invalid in BookSeats request", LoggerNameForCustomLog,
                exception: null,
                context: new Dictionary<string, object>
                {
                    { "Path", HttpContext.Request.Path.ToString() }
                });
            return Unauthorized("User ID claim is missing or invalid.");
        }

        var requestJsonForSerilogInfo = System.Text.Json.JsonSerializer.Serialize(requests);
        _logger.LogInformation("Starting to book seats for User {UserAccountId}. Request: {requests}", userAccountId, requestJsonForSerilogInfo);

        _customLogger.Info("Starting to book seats", LoggerNameForCustomLog,
            context: new Dictionary<string, object>
            {
                { "UserAccountId", userAccountId },
                { "BookSeatRequests", requests }
            });

        var result = await _ticketBookingService.BookPlaces(userAccountId, requests);

        _logger.LogInformation("Successfully booked seats for User {UserAccountId}, got SeatLockId {SeatLockId}. OriginalRequest: {requests}",
            userAccountId, result, requestJsonForSerilogInfo);

        _customLogger.Info("Successfully booked seats", LoggerNameForCustomLog,
            context: new Dictionary<string, object>
            {
                { "UserAccountId", userAccountId },
                { "SeatLockId", result },
                { "OriginalRequests", requests }
            });

        return Ok(result);
    }

    [HttpPost("cancel-books")] 
    public async Task<ActionResult<bool>> CancelBookSeats([FromBody] Guid seatLockId) 
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userAccountId))
        {
            _logger.LogWarning("User ID claim is missing or invalid in CancelBookSeats request for SeatLockId {SeatLockId}. Path: {Path}", seatLockId, HttpContext.Request.Path);

            _customLogger.Warn("User ID claim is missing or invalid in CancelBookSeats request", LoggerNameForCustomLog,
                exception: null,
                context: new Dictionary<string, object>
                {
                    { "SeatLockId", seatLockId },
                    { "Path", HttpContext.Request.Path.ToString() }
                });
            return Unauthorized("User ID claim is missing or invalid.");
        }

        _logger.LogInformation("Attempting to cancel seat lock {SeatLockId} for User {UserAccountId}", seatLockId, userAccountId);

        _customLogger.Info("Attempting to cancel seat lock", LoggerNameForCustomLog,
            context: new Dictionary<string, object>
            {
                { "UserAccountId", userAccountId },
                { "SeatLockId", seatLockId }
            });

        var result = await _ticketBookingService.CancelBookPlaces(userAccountId, seatLockId);

        if (result)
        {
            _logger.LogInformation("Successfully canceled seat lock {SeatLockId} for User {UserAccountId}", seatLockId, userAccountId);
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
            _logger.LogWarning("Failed to cancel seat lock {SeatLockId} for User {UserAccountId}. Service returned {ServiceResult}", seatLockId, userAccountId, result);
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
    public async Task<ActionResult<List<SeatLockResponse>>> GetMyBookings()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userAccountId))
        {
            _logger.LogWarning("User ID claim is missing or invalid in GetMyBookings request. Path: {Path}", HttpContext.Request.Path);
            _customLogger.Warn("User ID claim is missing or invalid in GetMyBookings request", LoggerNameForCustomLog,
                exception: null,
                context: new Dictionary<string, object>
                {
                    { "Path", HttpContext.Request.Path.ToString() }
                });
            return Unauthorized("User ID claim is missing or invalid");
        }

        _logger.LogInformation("Attempting to get seat locks for User {UserAccountId}", userAccountId);

        _customLogger.Info("Attempting to get seat locks for user", LoggerNameForCustomLog,
            context: new Dictionary<string, object>
            {
                { "UserAccountId", userAccountId }
            });

        var result = await _ticketBookingService.GetBooks(userAccountId);

        _logger.LogInformation("Successfully retrieved {SeatLockCount} seat locks for User {UserAccountId}", result.Count(), userAccountId);

        _customLogger.Info("Successfully retrieved seat locks for user", LoggerNameForCustomLog,
            context: new Dictionary<string, object>
            {
                { "UserAccountId", userAccountId },
                { "SeatLockCount", result.Count() },
            });
        return Ok(result.ToList());
    }
}