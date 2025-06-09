using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces.IServices.AdminServices;

namespace RailwayApp.Web.Controllers.AdminControllers;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminCarriageAvailabilitiesController(
    IAdminCarriageAvailabilityService adminCarriageAvailabilityService,
    ILogger<AdminCarriageAvailabilitiesController> logger)
    : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CarriageAvailability>>> GetAllCarriageAvailabilities()
    {
        var userLogin = User.Identity?.Name ?? "Unknown";
        logger.LogInformation("Admin: Attempting to retrieve all carriage availabilities. User: {User}", userLogin);
        var items = await adminCarriageAvailabilityService.GetAllItems();
        logger.LogInformation("Admin: Successfully retrieved {Count} carriage availabilities. User: {User}", items.Count(), userLogin);
        return Ok(items);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CarriageAvailability>> GetCarriageAvailabilityById(Guid id)
    {
        var userLogin = User.Identity?.Name ?? "Unknown";
        logger.LogInformation("Admin: Attempting to retrieve carriage availability with ID: {Id}. User: {User}", id, userLogin);
        
        var carriageAvailability = await adminCarriageAvailabilityService.GetItemByIdAsync(id); 
        
        logger.LogInformation("Admin: Successfully retrieved carriage availability with ID: {Id}. User: {User}", id, userLogin);
        return Ok(carriageAvailability);
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> CreateCarriageAvailability([FromBody] CarriageAvailability carriageAvailability)
    {
        var userLogin = User.Identity?.Name ?? "Unknown";
        if (!ModelState.IsValid)
        {
            logger.LogWarning("Admin: Model validation failed for creating carriage availability: {ModelStateErrors}. Request: {Request}. User: {User}",
                JsonSerializer.Serialize(ModelState), JsonSerializer.Serialize(carriageAvailability), userLogin);
            return BadRequest(ModelState);
        }

        logger.LogInformation("Admin: Attempting to create carriage availability: {CarriageAvailability}. User: {User}", JsonSerializer.Serialize(carriageAvailability), userLogin);
        var newId = await adminCarriageAvailabilityService.CreateItem(carriageAvailability);
        logger.LogInformation("Admin: Successfully created carriage availability with ID: {Id}. User: {User}", newId, userLogin);
        return CreatedAtAction(nameof(GetCarriageAvailabilityById), new { id = newId }, newId);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateCarriageAvailability(Guid id, [FromBody] CarriageAvailability carriageAvailability)
    {
        var userLogin = User.Identity?.Name ?? "Unknown";
        if (!ModelState.IsValid)
        {
            logger.LogWarning("Admin: Model validation failed for updating carriage availability ID {Id}: {ModelStateErrors}. Request: {Request}. User: {User}",
                id, JsonSerializer.Serialize(ModelState), JsonSerializer.Serialize(carriageAvailability), userLogin);
            return BadRequest(ModelState);
        }

        if (id != carriageAvailability.Id)
        {
            logger.LogWarning("Admin: Mismatch between ID from URL ({UrlId}) and payload ID ({PayloadId}) for updating carriage availability. User: {User}",
                id, carriageAvailability.Id, userLogin);
            return BadRequest(new ProblemDetails 
            { 
                Title = "ID Mismatch", 
                Detail = "ID in URL must match ID in request body.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        logger.LogInformation("Admin: Attempting to update carriage availability with ID: {Id}. Data: {CarriageAvailability}. User: {User}", id, JsonSerializer.Serialize(carriageAvailability), userLogin);
        await adminCarriageAvailabilityService.UpdateItem(id, carriageAvailability);
        logger.LogInformation("Admin: Successfully updated carriage availability with ID: {Id}. User: {User}", id, userLogin);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteCarriageAvailability(Guid id)
    {
        var userLogin = User.Identity?.Name ?? "Unknown";
        logger.LogInformation("Admin: Attempting to delete carriage availability with ID: {Id}. User: {User}", id, userLogin);
        
        await adminCarriageAvailabilityService.DeleteItem(id);
        
        logger.LogInformation("Admin: Successfully deleted carriage availability with ID: {Id}. User: {User}", id, userLogin);
        return NoContent();
    }
}