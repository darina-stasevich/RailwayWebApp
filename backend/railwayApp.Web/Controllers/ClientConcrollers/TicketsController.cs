using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RailwayApp.Application.Models;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces.IServices;

namespace RailwayApp.Web.ClientControllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Client")]
public class TicketsController(ILogger<TicketsController> logger, ITicketService ticketService) : ControllerBase
{
    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<TicketDto>>> GetActiveTickets()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userAccountId))
            return Unauthorized("User ID claim is missing or invalid.");

        var tickets = await ticketService.GetActiveTickets(userAccountId);
        logger.LogInformation("successfully get active tickets for user {userAccountId}", userAccountId);
        return tickets.ToList();
    }

    [HttpGet("cancelled")]
    public async Task<ActionResult<IEnumerable<TicketDto>>> GetCancelledTickets()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userAccountId))
            return Unauthorized("User ID claim is missing or invalid.");

        var tickets = await ticketService.GetCancelledTickets(userAccountId);
        logger.LogInformation("successfully get cancelled tickets for user {userAccountId}", userAccountId);
        return tickets.ToList();
    }

    [HttpGet("expired")]
    public async Task<ActionResult<IEnumerable<TicketDto>>> GetExpiredTickets()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userAccountId))
            return Unauthorized("User ID claim is missing or invalid.");

        var tickets = await ticketService.GetExpiredTickets(userAccountId);
        logger.LogInformation("successfully get expired tickets for user {userAccountId}", userAccountId);
        return tickets.ToList();
    }
}