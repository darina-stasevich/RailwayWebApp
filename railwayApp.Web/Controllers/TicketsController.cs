using Microsoft.AspNetCore.Mvc;
using RailwayApp.Domain;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces.IServices;

namespace RailwayApp.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TicketsController(ILogger<TicketsController> logger, ITicketService ticketService) : ControllerBase
{
    [HttpPost("active")]
    public async Task<ActionResult<IEnumerable<Ticket>>> GetActiveTickets()
    {
        var userAccountId = Guid.Parse("212ac631-4f3d-4010-adfd-e4123d569a91"); // replace with normal claim
        try
        {
            var tickets = await ticketService.GetActiveTickets(userAccountId);
            logger.LogInformation("successfully get active tickets for user {userAccountId}", userAccountId);
            return tickets.ToList();
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
        catch (Exception ex)
        {
            logger.LogError("Unhandled exception caught {ex}", ex.Message);
            return StatusCode(500, "Internal server error");
        }
    }
    
    [HttpPost("cancelled")]
    public async Task<ActionResult<IEnumerable<Ticket>>> GetCancelledTickets()
    {
        var userAccountId = Guid.Parse("212ac631-4f3d-4010-adfd-e4123d569a91"); // replace with normal claim
        try
        {
            var tickets = await ticketService.GetCancelledTickets(userAccountId);
            logger.LogInformation("successfully get cancelled tickets for user {userAccountId}", userAccountId);
            return tickets.ToList();
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
        catch (Exception ex)
        {
            logger.LogError("Unhandled exception caught {ex}", ex.Message);
            return StatusCode(500, "Internal server error");
        }
    }
    
    [HttpPost("expired")]
    public async Task<ActionResult<IEnumerable<Ticket>>> GetExpiredTickets()
    {
        var userAccountId = Guid.Parse("212ac631-4f3d-4010-adfd-e4123d569a91"); // replace with normal claim
        try
        {
            var tickets = await ticketService.GetExpiredTickets(userAccountId);
            logger.LogInformation("successfully get expired tickets for user {userAccountId}", userAccountId);
            return tickets.ToList();
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
        catch (Exception ex)
        {
            logger.LogError("Unhandled exception caught {ex}", ex.Message);
            return StatusCode(500, "Internal server error");
        }
    }
}