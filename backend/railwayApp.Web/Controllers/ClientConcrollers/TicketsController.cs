using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using railway_service;
using RailwayApp.Application.Models; // Для TicketDto
using RailwayApp.Domain.Interfaces.IServices; // Для ITicketService
// Предполагается, что IMyCustomLogger и его реализация (MyCustomFileJsonLogger) определены
// и зарегистрированы в DI.
// Например, в Program.cs:
// builder.Services.AddSingleton<IMyCustomLogger, MyCustomFileJsonLogger>();

namespace RailwayApp.Web.ClientControllers; // Обрати внимание на пространство имен

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Client")]
public class TicketsController(
    ILogger<TicketsController> logger,
    ITicketService ticketService,
    IMyCustomLogger customLogger)
    : ControllerBase
{
    private const string LoggerNameForCustomLog = "TicketsController"; // Имя для кастомного логгера

    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<TicketDto>>> GetActiveTickets()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userAccountId))
        {
            // Стандартное логирование (Serilog)
            logger.LogWarning("User ID claim is missing or invalid in GetActiveTickets request. Path: {Path}", HttpContext.Request.Path);
            // Твое кастомное логирование
            customLogger.Warn("User ID claim is missing or invalid in GetActiveTickets request", LoggerNameForCustomLog,
                exception: null,
                context: new Dictionary<string, object> { { "Path", HttpContext.Request.Path.ToString() } });
            return Unauthorized("User ID claim is missing or invalid.");
        }

        // Стандартное логирование (Serilog) - перед вызовом
        logger.LogInformation("Attempting to get active tickets for User {UserAccountId}", userAccountId);
        // Твое кастомное логирование - перед вызовом
        customLogger.Info("Attempting to get active tickets for user", LoggerNameForCustomLog,
            context: new Dictionary<string, object> { { "UserAccountId", userAccountId } });

        var tickets = await ticketService.GetActiveTickets(userAccountId);
        var ticketList = tickets.ToList(); // Материализуем для подсчета и возврата

        // Стандартное логирование (Serilog) - после вызова
        logger.LogInformation("Successfully retrieved {TicketCount} active tickets for User {UserAccountId}", ticketList.Count, userAccountId);
        // Твое кастомное логирование - после вызова
        customLogger.Info("Successfully retrieved active tickets for user", LoggerNameForCustomLog,
            context: new Dictionary<string, object>
            {
                { "UserAccountId", userAccountId },
                { "ActiveTicketCount", ticketList.Count }
                // { "ActiveTickets", ticketList } // Раскомментируй с осторожностью, если список может быть большим
            });
        return Ok(ticketList);
    }

    [HttpGet("cancelled")]
    public async Task<ActionResult<IEnumerable<TicketDto>>> GetCancelledTickets()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userAccountId))
        {
            // Стандартное логирование (Serilog)
            logger.LogWarning("User ID claim is missing or invalid in GetCancelledTickets request. Path: {Path}", HttpContext.Request.Path);
            // Твое кастомное логирование
            customLogger.Warn("User ID claim is missing or invalid in GetCancelledTickets request", LoggerNameForCustomLog,
                exception: null,
                context: new Dictionary<string, object> { { "Path", HttpContext.Request.Path.ToString() } });
            return Unauthorized("User ID claim is missing or invalid.");
        }

        // Стандартное логирование (Serilog) - перед вызовом
        logger.LogInformation("Attempting to get cancelled tickets for User {UserAccountId}", userAccountId);
        // Твое кастомное логирование - перед вызовом
        customLogger.Info("Attempting to get cancelled tickets for user", LoggerNameForCustomLog,
            context: new Dictionary<string, object> { { "UserAccountId", userAccountId } });

        var tickets = await ticketService.GetCancelledTickets(userAccountId);
        var ticketList = tickets.ToList();

        // Стандартное логирование (Serilog) - после вызова
        logger.LogInformation("Successfully retrieved {TicketCount} cancelled tickets for User {UserAccountId}", ticketList.Count, userAccountId);
        // Твое кастомное логирование - после вызова
        customLogger.Info("Successfully retrieved cancelled tickets for user", LoggerNameForCustomLog,
            context: new Dictionary<string, object>
            {
                { "UserAccountId", userAccountId },
                { "CancelledTicketCount", ticketList.Count }
                // { "CancelledTickets", ticketList } // Раскомментируй с осторожностью
            });
        return Ok(ticketList);
    }

    [HttpGet("expired")]
    public async Task<ActionResult<IEnumerable<TicketDto>>> GetExpiredTickets()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userAccountId))
        {
            // Стандартное логирование (Serilog)
            logger.LogWarning("User ID claim is missing or invalid in GetExpiredTickets request. Path: {Path}", HttpContext.Request.Path);
            // Твое кастомное логирование
            customLogger.Warn("User ID claim is missing or invalid in GetExpiredTickets request", LoggerNameForCustomLog,
                exception: null,
                context: new Dictionary<string, object> { { "Path", HttpContext.Request.Path.ToString() } });
            return Unauthorized("User ID claim is missing or invalid.");
        }

        // Стандартное логирование (Serilog) - перед вызовом
        logger.LogInformation("Attempting to get expired tickets for User {UserAccountId}", userAccountId);
        // Твое кастомное логирование - перед вызовом
        customLogger.Info("Attempting to get expired tickets for user", LoggerNameForCustomLog,
            context: new Dictionary<string, object> { { "UserAccountId", userAccountId } });

        var tickets = await ticketService.GetExpiredTickets(userAccountId);
        var ticketList = tickets.ToList();

        // Стандартное логирование (Serilog) - после вызова
        logger.LogInformation("Successfully retrieved {TicketCount} expired tickets for User {UserAccountId}", ticketList.Count, userAccountId);
        // Твое кастомное логирование - после вызова
        customLogger.Info("Successfully retrieved expired tickets for user", LoggerNameForCustomLog,
            context: new Dictionary<string, object>
            {
                { "UserAccountId", userAccountId },
                { "ExpiredTicketCount", ticketList.Count }
                // { "ExpiredTickets", ticketList } // Раскомментируй с осторожностью
            });
        return Ok(ticketList);
    }
}