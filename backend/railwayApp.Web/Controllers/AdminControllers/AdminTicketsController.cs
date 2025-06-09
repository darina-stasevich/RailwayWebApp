using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces.IServices.AdminServices;

namespace RailwayApp.Web.Controllers.AdminControllers;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminTicketsController(
    IAdminTicketService adminTicketService,
    ILogger<AdminTicketsController> logger)
    : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Ticket>>> GetAllTickets()
    {
        logger.LogInformation("Admin: User attempting to retrieve all tickets.");
        var items = await adminTicketService.GetAllItems();
        logger.LogInformation("Admin: User successfully retrieved {Count} tickets.", items.Count());
        return Ok(items);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Ticket>> GetTicketById(Guid id)
    {
        logger.LogInformation("Admin: User attempting to retrieve ticket with ID: {Id}.", id);
        
        var ticket = await adminTicketService.GetItemByIdAsync(id); 
        
        logger.LogInformation("Admin: User successfully retrieved ticket with ID: {Id}.", id);
        return Ok(ticket);
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> CreateTicket([FromBody] Ticket ticket)
    {
        if (!ModelState.IsValid)
        {
            logger.LogWarning("Admin: User - Model validation failed for creating ticket: {ModelStateErrors}. Request: {Request}.",
                JsonSerializer.Serialize(ModelState), JsonSerializer.Serialize(ticket));
            return BadRequest(ModelState);
        }

        logger.LogInformation("Admin: User attempting to create ticket: {Ticket}.", JsonSerializer.Serialize(ticket));
        var newId = await adminTicketService.CreateItem(ticket);
        logger.LogInformation("Admin: User successfully created ticket with ID: {Id}.", newId);
        return CreatedAtAction(nameof(GetTicketById), new { id = newId }, newId);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateTicket(Guid id, [FromBody] Ticket ticket)
    {
        if (!ModelState.IsValid)
        {
            logger.LogWarning("Admin: User - Model validation failed for updating ticket ID {Id}: {ModelStateErrors}. Request: {Request}.",
                id, JsonSerializer.Serialize(ModelState), JsonSerializer.Serialize(ticket));
            return BadRequest(ModelState);
        }

        if (id != ticket.Id)
        {
            logger.LogWarning("Admin: User - Mismatch between ID from URL ({UrlId}) and payload ID ({PayloadId}) for updating ticket.",
                id, ticket.Id);
            return BadRequest(new ProblemDetails 
            { 
                Title = "ID Mismatch", 
                Detail = "ID in URL must match ID in request body.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        logger.LogInformation("Admin: User attempting to update ticket with ID: {Id}. Data: {Ticket}.", id, JsonSerializer.Serialize(ticket));
        await adminTicketService.UpdateItem(id, ticket);
        logger.LogInformation("Admin: User successfully updated ticket with ID: {Id}.", id);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteTicket(Guid id)
    {
        logger.LogInformation("Admin: User attempting to delete ticket with ID: {Id}.", id);
        await adminTicketService.DeleteItem(id);
        logger.LogInformation("Admin: User successfully deleted ticket with ID: {Id}.", id);
        return NoContent();
    }
}