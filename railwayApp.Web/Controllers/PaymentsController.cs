using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RailwayApp.Domain;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces.IServices;

namespace RailwayApp.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Client")]
public class PaymentsController(IPaymentService paymentService, ILogger<PaymentsController> logger) : ControllerBase
{
    [HttpPost("pay")]
    public async Task<ActionResult<List<Ticket>>> PaySeatLock(Guid seatLockId)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out Guid userAccountId)) 
            return Unauthorized("User ID claim is missing or invalid.");
        
        try
        {
            var paymentResult = await paymentService.PayTickets(userAccountId, seatLockId);
            
            logger.LogInformation("successfully pay seats for seatlock {seatLockId}", seatLockId);

            return Ok(paymentResult);
        }
        catch (UserServiceUserNotFoundException ex)
        {
            logger.LogWarning("user with id {id} not found", userAccountId);
            return NotFound(ex.Message);
        }
        catch (UserServiceUserBlockedException ex)
        {
            logger.LogWarning("user with id {id} blocked", userAccountId);
            return BadRequest(ex.Message);
        }
        catch (TicketBookingServiceSeatNotAvailableException ex)
        {
            logger.LogWarning("one or more required seats is already blocked: {ex}", ex.Message);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError("Unhandled exception caught {ex}", ex.Message);
            return StatusCode(500, ex.Message); // change at normal
        }

    }
    
    [HttpPost("cancelPay")]
    public async Task<ActionResult<List<Ticket>>> CancelPayTicket(Guid ticketId)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out Guid userAccountId)) 
            return Unauthorized("User ID claim is missing or invalid.");
        
        try
        {
            var cancelPaymentResult = await paymentService.CancelTicket(userAccountId, ticketId);
            
            logger.LogInformation("successfully cancel ticket {ticketId}", ticketId);

            return Ok(cancelPaymentResult);
        }
        catch (UserServiceUserNotFoundException ex)
        {
            logger.LogWarning("user with id {id} not found", userAccountId);
            return NotFound(ex.Message);
        }
        catch (UserServiceUserBlockedException ex)
        {
            logger.LogWarning("user with id {id} blocked", userAccountId);
            return BadRequest(ex.Message);
        }
        catch (TicketBookingServiceSeatNotAvailableException ex)
        {
            logger.LogWarning("one or more required seats is already blocked: {ex}", ex.Message);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError("Unhandled exception caught {ex}", ex.Message);
            return StatusCode(500, ex.Message); // change at normal
        }

    }
}