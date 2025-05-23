using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using railway_service;
using RailwayApp.Domain.Entities; // Для Ticket
using RailwayApp.Domain.Interfaces.IServices; // Для IPaymentService
// Предполагается, что IMyCustomLogger и его реализация (MyCustomFileJsonLogger) определены
// и зарегистрированы в DI.
// Например, в Program.cs:
// builder.Services.AddSingleton<IMyCustomLogger, MyCustomFileJsonLogger>();

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
            // Стандартное логирование (Serilog)
            logger.LogWarning("User ID claim is missing or invalid in PaySeatLock request for SeatLockId {SeatLockId}. Path: {Path}", seatLockId, HttpContext.Request.Path);

            // Твое кастомное логирование
            customLogger.Warn("User ID claim is missing or invalid in PaySeatLock request", LoggerNameForCustomLog,
                exception: null,
                context: new Dictionary<string, object>
                {
                    { "SeatLockId", seatLockId },
                    { "Path", HttpContext.Request.Path.ToString() }
                });
            return Unauthorized("User ID claim is missing or invalid.");
        }

        // Стандартное логирование (Serilog) - перед вызовом сервиса
        logger.LogInformation("Attempting to pay for SeatLockId {SeatLockId} by User {UserAccountId}", seatLockId, userAccountId);

        // Твое кастомное логирование - перед вызовом сервиса
        customLogger.Info("Attempting to pay for seat lock", LoggerNameForCustomLog,
            context: new Dictionary<string, object>
            {
                { "UserAccountId", userAccountId },
                { "SeatLockId", seatLockId }
            });

        var paymentResult = await paymentService.PayTickets(userAccountId, seatLockId);

        // Стандартное логирование (Serilog) - после вызова сервиса
        logger.LogInformation("Successfully processed payment for SeatLockId {SeatLockId} by User {UserAccountId}. Tickets generated: {TicketCount}",
            seatLockId, userAccountId, paymentResult?.Count ?? 0);

        // Твое кастомное логирование - после вызова сервиса
        customLogger.Info("Successfully processed payment for seat lock", LoggerNameForCustomLog,
            context: new Dictionary<string, object>
            {
                { "UserAccountId", userAccountId },
                { "SeatLockId", seatLockId },
                { "TicketsGeneratedCount", paymentResult?.Count ?? 0 },
                // Можно добавить ID сгенерированных билетов, если это полезно и не слишком много данных
                // { "GeneratedTicketIds", paymentResult?.Select(t => t.Id).ToList() }
            });

        return Ok(paymentResult);
    }

    [HttpPost("cancel-pay")] // Изменил cancelPay на cancel-pay для единообразия
    public async Task<ActionResult<List<Ticket>>> CancelPayTicket([FromBody] Guid ticketId) // Предполагаем, что это ID одного билета
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userAccountId))
        {
            // Стандартное логирование (Serilog)
            logger.LogWarning("User ID claim is missing or invalid in CancelPayTicket request for TicketId {TicketId}. Path: {Path}", ticketId, HttpContext.Request.Path);

            // Твое кастомное логирование
            customLogger.Warn("User ID claim is missing or invalid in CancelPayTicket request", LoggerNameForCustomLog,
                exception: null,
                context: new Dictionary<string, object>
                {
                    { "TicketId", ticketId },
                    { "Path", HttpContext.Request.Path.ToString() }
                });
            return Unauthorized("User ID claim is missing or invalid.");
        }

        // Стандартное логирование (Serilog) - перед вызовом сервиса
        logger.LogInformation("Attempting to cancel payment for TicketId {TicketId} by User {UserAccountId}", ticketId, userAccountId);

        // Твое кастомное логирование - перед вызовом сервиса
        customLogger.Info("Attempting to cancel payment for ticket", LoggerNameForCustomLog,
            context: new Dictionary<string, object>
            {
                { "UserAccountId", userAccountId },
                { "TicketId", ticketId }
            });

        // Предполагаем, что CancelTicket возвращает список затронутых/возвращенных билетов или null/пустой список при неудаче
        var cancelPaymentResult = await paymentService.CancelTicket(userAccountId, ticketId);

        // Стандартное логирование (Serilog) - после вызова сервиса
        logger.LogInformation("Successfully processed payment cancellation for TicketId {TicketId} by User {UserAccountId}",
            ticketId, userAccountId);

        // Твое кастомное логирование - после вызова сервиса
        customLogger.Info("Successfully processed payment cancellation for ticket", LoggerNameForCustomLog,
            context: new Dictionary<string, object>
            {
                { "UserAccountId", userAccountId },
                { "TicketId", ticketId },
                // { "ResultingTicketIds", cancelPaymentResult?.Select(t => t.Id).ToList() } // Если это полезно
            });

        return Ok(cancelPaymentResult);
    }
}