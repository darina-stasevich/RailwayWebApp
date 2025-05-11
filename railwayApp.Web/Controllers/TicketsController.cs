using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces.IServices;

namespace RailwayApp.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Client")]
public class TicketsController(ILogger<TicketsController> logger, ITicketService ticketService) : ControllerBase
{
    [HttpPost("active")]
    public async Task<ActionResult<IEnumerable<Ticket>>> GetActiveTickets()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userAccountId))
            return Unauthorized("User ID claim is missing or invalid.");

        var tickets = await ticketService.GetActiveTickets(userAccountId);
        logger.LogInformation("successfully get active tickets for user {userAccountId}", userAccountId);
        return tickets.ToList();
    }

    [HttpPost("cancelled")]
    public async Task<ActionResult<IEnumerable<Ticket>>> GetCancelledTickets()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userAccountId))
            return Unauthorized("User ID claim is missing or invalid.");

        var tickets = await ticketService.GetCancelledTickets(userAccountId);
        logger.LogInformation("successfully get cancelled tickets for user {userAccountId}", userAccountId);
        return tickets.ToList();
    }

    [HttpPost("expired")]
    public async Task<ActionResult<IEnumerable<Ticket>>> GetExpiredTickets()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userAccountId))
            return Unauthorized("User ID claim is missing or invalid.");

        var tickets = await ticketService.GetExpiredTickets(userAccountId);
        logger.LogInformation("successfully get expired tickets for user {userAccountId}", userAccountId);
        return tickets.ToList();
    }
}