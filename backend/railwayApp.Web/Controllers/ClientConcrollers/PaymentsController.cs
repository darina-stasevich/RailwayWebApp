using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces.IServices;

namespace RailwayApp.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Client")]
public class PaymentsController(IPaymentService paymentService, ILogger<PaymentsController> logger) : ControllerBase
{
    [HttpPost("pay")]
    public async Task<ActionResult<List<Ticket>>> PaySeatLock([FromBody] Guid seatLockId)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userAccountId))
            return Unauthorized("User ID claim is missing or invalid.");
        var paymentResult = await paymentService.PayTickets(userAccountId, seatLockId);

        logger.LogInformation("successfully pay seats for seatlock {seatLockId}", seatLockId);

        return Ok(paymentResult);
    }

    [HttpPost("cancelPay")]
    public async Task<ActionResult<List<Ticket>>> CancelPayTicket(Guid ticketId)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userAccountId))
            return Unauthorized("User ID claim is missing or invalid.");

        var cancelPaymentResult = await paymentService.CancelTicket(userAccountId, ticketId);

        logger.LogInformation("successfully cancel ticket {ticketId}", ticketId);

        return Ok(cancelPaymentResult);
    }
}