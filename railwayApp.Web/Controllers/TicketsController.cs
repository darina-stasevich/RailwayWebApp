using Microsoft.AspNetCore.Mvc;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces.IRepositories;

namespace railway_service.Controllers;

// TicketsController.cs
[ApiController]
[Route("api/tickets")]
public class TicketsController(ITicketRepository ticketRepository,
    IUserAccountRepository userAccountRepository) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateTicket([FromBody] Ticket ticket)
    {
            // Проверка валидности модели
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Проверка существования пользователя
            var user = await userAccountRepository.GetByEmailAsync(ticket.UserAccountEmail);
            if (user == null)
            {
                return NotFound($"User with email {ticket.UserAccountEmail} not found.");
            }

        
            // Сохранение билета
            await ticketRepository.CreateAsync(ticket);

            return CreatedAtAction(
                nameof(GetById),
                new { id = ticket.Id },
                ticket
            );
    }

    [HttpGet("by-email/{email}")]
    public async Task<IActionResult> GetByEmail(string email)
    {
        var user = await userAccountRepository.GetByEmailAsync(email);
        if (user == null)
        {
            return NotFound("User not found.");
        }

        var tickets = await ticketRepository.GetByUserEmailAsync(email);
        return Ok(tickets ?? new List<Ticket>());
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var ticket = await ticketRepository.GetByIdAsync(id);
        return ticket == null ? NotFound() : Ok(ticket);
    }
}