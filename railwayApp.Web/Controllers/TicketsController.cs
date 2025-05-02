using Microsoft.AspNetCore.Mvc;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces.IRepositories;

namespace railway_service.Controllers;

// TicketsController.cs
[ApiController]
[Route("api/tickets")]
public class TicketsController(
    ITicketRepository ticketRepository,
    IUserAccountRepository userAccountRepository,
    ILogger<TicketsController> logger) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateTicket([FromBody] Ticket ticket)
    {
        logger.LogInformation(@"Creating ticket {ticket}", ticket);
        
        if (!ModelState.IsValid)
        {
            logger.LogWarning("Incorrect model: {@Errors}", ModelState.Values.SelectMany(v => v.Errors));
            return BadRequest(ModelState);
        }

        try
        {
            var user = await userAccountRepository.GetByEmailAsync(ticket.UserAccountEmail);
            if (user == null)
            {
                logger.LogWarning("Incorrect email: {@Email}", ticket.UserAccountEmail);
                return NotFound($"User with email {ticket.UserAccountEmail} not found.");
            }

            // Сохранение билета
            await ticketRepository.CreateAsync(ticket); 
            logger.LogInformation(@"Ticket {ticket} created", ticket);
            
            return CreatedAtAction(
                nameof(GetById),
                new { id = ticket.Id },
                ticket
            );
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating ticket {ticket}", ticket);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("by-email/{email}")]
    public async Task<IActionResult> GetByEmail(string email)
    {
        try
        {
            var user = await userAccountRepository.GetByEmailAsync(email);
            if (user == null)
            {   
                logger.LogWarning("Incorrect email: {@email}", email);
                return NotFound("User not found.");
            }

            var tickets = await ticketRepository.GetByUserEmailAsync(email);
            logger.LogInformation(@"Received tickets for {email}", email);
            return Ok(tickets ?? new List<Ticket>());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting tickets by {email}", email);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var ticket = await ticketRepository.GetByIdAsync(id);
            logger.LogInformation(@"Received ticket {id}", id);
            return Ok(ticket);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting ticket {id}", id);
            return StatusCode(500, "Internal server error");
        }
    }
}