using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using railway_service;
using RailwayApp.Application.Models;
using RailwayApp.Domain.Interfaces.IServices;


namespace RailwayApp.Web.ClientControllers; 

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Client")]
public class TicketsController(
    ILogger<TicketsController> logger,
    ITicketService ticketService,
    IMyCustomLogger customLogger)
    : ControllerBase
{
    private const string LoggerNameForCustomLog = "TicketsController";

    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<TicketDto>>> GetActiveTickets()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userAccountId))
        {
            logger.LogWarning("User ID claim is missing or invalid in GetActiveTickets request. Path: {Path}", HttpContext.Request.Path);
            customLogger.Warn("User ID claim is missing or invalid in GetActiveTickets request", LoggerNameForCustomLog,
                exception: null,
                context: new Dictionary<string, object> { { "Path", HttpContext.Request.Path.ToString() } });
            return Unauthorized("User ID claim is missing or invalid.");
        }

        logger.LogInformation("Attempting to get active tickets for User {UserAccountId}", userAccountId);
        customLogger.Info("Attempting to get active tickets for user", LoggerNameForCustomLog,
            context: new Dictionary<string, object> { { "UserAccountId", userAccountId } });

        var tickets = await ticketService.GetActiveTickets(userAccountId);
        var ticketList = tickets.ToList();
        
        logger.LogInformation("Successfully retrieved {TicketCount} active tickets for User {UserAccountId}", ticketList.Count, userAccountId);
        customLogger.Info("Successfully retrieved active tickets for user", LoggerNameForCustomLog,
            context: new Dictionary<string, object>
            {
                { "UserAccountId", userAccountId },
                { "ActiveTicketCount", ticketList.Count }
            });
        return Ok(ticketList);
    }

    [HttpGet("cancelled")]
    public async Task<ActionResult<IEnumerable<TicketDto>>> GetCancelledTickets()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userAccountId))
        {
            logger.LogWarning("User ID claim is missing or invalid in GetCancelledTickets request. Path: {Path}", HttpContext.Request.Path);
            customLogger.Warn("User ID claim is missing or invalid in GetCancelledTickets request", LoggerNameForCustomLog,
                exception: null,
                context: new Dictionary<string, object> { { "Path", HttpContext.Request.Path.ToString() } });
            return Unauthorized("User ID claim is missing or invalid.");
        }

        logger.LogInformation("Attempting to get cancelled tickets for User {UserAccountId}", userAccountId);
        customLogger.Info("Attempting to get cancelled tickets for user", LoggerNameForCustomLog,
            context: new Dictionary<string, object> { { "UserAccountId", userAccountId } });

        var tickets = await ticketService.GetCancelledTickets(userAccountId);
        var ticketList = tickets.ToList();

        logger.LogInformation("Successfully retrieved {TicketCount} cancelled tickets for User {UserAccountId}", ticketList.Count, userAccountId);
        customLogger.Info("Successfully retrieved cancelled tickets for user", LoggerNameForCustomLog,
            context: new Dictionary<string, object>
            {
                { "UserAccountId", userAccountId },
                { "CancelledTicketCount", ticketList.Count }
            });
        return Ok(ticketList);
    }

    [HttpGet("expired")]
    public async Task<ActionResult<IEnumerable<TicketDto>>> GetExpiredTickets()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userAccountId))
        {
            logger.LogWarning("User ID claim is missing or invalid in GetExpiredTickets request. Path: {Path}", HttpContext.Request.Path);
            customLogger.Warn("User ID claim is missing or invalid in GetExpiredTickets request", LoggerNameForCustomLog,
                exception: null,
                context: new Dictionary<string, object> { { "Path", HttpContext.Request.Path.ToString() } });
            return Unauthorized("User ID claim is missing or invalid.");
        }

        logger.LogInformation("Attempting to get expired tickets for User {UserAccountId}", userAccountId);
        customLogger.Info("Attempting to get expired tickets for user", LoggerNameForCustomLog,
            context: new Dictionary<string, object> { { "UserAccountId", userAccountId } });

        var tickets = await ticketService.GetExpiredTickets(userAccountId);
        var ticketList = tickets.ToList();

        logger.LogInformation("Successfully retrieved {TicketCount} expired tickets for User {UserAccountId}", ticketList.Count, userAccountId);
        customLogger.Info("Successfully retrieved expired tickets for user", LoggerNameForCustomLog,
            context: new Dictionary<string, object>
            {
                { "UserAccountId", userAccountId },
                { "ExpiredTicketCount", ticketList.Count }
            });
        return Ok(ticketList);
    }
}