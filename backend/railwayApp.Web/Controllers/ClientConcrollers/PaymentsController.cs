using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using railway_service;
using RailwayApp.Domain.Entities; 
using RailwayApp.Domain.Interfaces.IServices;

namespace RailwayApp.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Client")]
public class PaymentsController(
    IPaymentService paymentService,
    ILogger<PaymentsController> logger,
    IMyCustomLogger customLogger)
    : ControllerBase
{
    private const string LoggerNameForCustomLog = "PaymentsController";

    [HttpPost("pay")]
    public async Task<ActionResult<List<Ticket>>> PaySeatLock([FromBody] Guid seatLockId)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userAccountId))
        {
            logger.LogWarning("User ID claim is missing or invalid in PaySeatLock request for SeatLockId {SeatLockId}. Path: {Path}", seatLockId, HttpContext.Request.Path);

            customLogger.Warn("User ID claim is missing or invalid in PaySeatLock request", LoggerNameForCustomLog,
                exception: null,
                context: new Dictionary<string, object>
                {
                    { "SeatLockId", seatLockId },
                    { "Path", HttpContext.Request.Path.ToString() }
                });
            return Unauthorized("User ID claim is missing or invalid.");
        }

        logger.LogInformation("Attempting to pay for SeatLockId {SeatLockId} by User {UserAccountId}", seatLockId, userAccountId);

        customLogger.Info("Attempting to pay for seat lock", LoggerNameForCustomLog,
            context: new Dictionary<string, object>
            {
                { "UserAccountId", userAccountId },
                { "SeatLockId", seatLockId }
            });

        var paymentResult = await paymentService.PayTickets(userAccountId, seatLockId);

        logger.LogInformation("Successfully processed payment for SeatLockId {SeatLockId} by User {UserAccountId}. Tickets generated: {TicketCount}",
            seatLockId, userAccountId, paymentResult?.Count ?? 0);

        customLogger.Info("Successfully processed payment for seat lock", LoggerNameForCustomLog,
            context: new Dictionary<string, object>
            {
                { "UserAccountId", userAccountId },
                { "SeatLockId", seatLockId },
                { "TicketsGeneratedCount", paymentResult?.Count ?? 0 },
            });

        return Ok(paymentResult);
    }

    [HttpPost("cancel-pay")]
    public async Task<ActionResult<List<Ticket>>> CancelPayTicket([FromBody] Guid ticketId) 
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userAccountId))
        {
            logger.LogWarning("User ID claim is missing or invalid in CancelPayTicket request for TicketId {TicketId}. Path: {Path}", ticketId, HttpContext.Request.Path);

            customLogger.Warn("User ID claim is missing or invalid in CancelPayTicket request", LoggerNameForCustomLog,
                exception: null,
                context: new Dictionary<string, object>
                {
                    { "TicketId", ticketId },
                    { "Path", HttpContext.Request.Path.ToString() }
                });
            return Unauthorized("User ID claim is missing or invalid.");
        }

        logger.LogInformation("Attempting to cancel payment for TicketId {TicketId} by User {UserAccountId}", ticketId, userAccountId);

        customLogger.Info("Attempting to cancel payment for ticket", LoggerNameForCustomLog,
            context: new Dictionary<string, object>
            {
                { "UserAccountId", userAccountId },
                { "TicketId", ticketId }
            });

        var cancelPaymentResult = await paymentService.CancelTicket(userAccountId, ticketId);

        logger.LogInformation("Successfully processed payment cancellation for TicketId {TicketId} by User {UserAccountId}",
            ticketId, userAccountId);

        customLogger.Info("Successfully processed payment cancellation for ticket", LoggerNameForCustomLog,
            context: new Dictionary<string, object>
            {
                { "UserAccountId", userAccountId },
                { "TicketId", ticketId },
            });

        return Ok(cancelPaymentResult);
    }
}