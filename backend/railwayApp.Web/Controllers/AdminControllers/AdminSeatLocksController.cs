using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces.IServices.AdminServices;

namespace RailwayApp.Web.Controllers.AdminControllers;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminSeatLocksController(
    IAdminSeatLockService adminSeatLockService,
    ILogger<AdminSeatLocksController> logger)
    : ControllerBase
{   [HttpGet]
    public async Task<ActionResult<IEnumerable<SeatLock>>> GetAllSeatLocks()
    {
        logger.LogInformation("Admin: User attempting to retrieve all seat locks.");
        var items = await adminSeatLockService.GetAllItems();
        logger.LogInformation("Admin: User successfully retrieved {Count} seat locks.", items.Count());
        return Ok(items);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SeatLock>> GetSeatLockById(Guid id)
    {
        logger.LogInformation("Admin: User attempting to retrieve seat lock with ID: {Id}.", id);
        
        var seatLock = await adminSeatLockService.GetItemByIdAsync(id); 
        
        logger.LogInformation("Admin: User successfully retrieved seat lock with ID: {Id}.", id);
        return Ok(seatLock);
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> CreateSeatLock([FromBody] SeatLock seatLock)
    {
        if (!ModelState.IsValid)
        {
            logger.LogWarning("Admin: User - Model validation failed for creating seat lock: {ModelStateErrors}. Request: {Request}.",
                JsonSerializer.Serialize(ModelState), JsonSerializer.Serialize(seatLock));
            return BadRequest(ModelState);
        }

        logger.LogInformation("Admin: User attempting to create seat lock: {SeatLock}.", JsonSerializer.Serialize(seatLock));
        var newId = await adminSeatLockService.CreateItem(seatLock);
        logger.LogInformation("Admin: User successfully created seat lock with ID: {Id}.", newId);
        return CreatedAtAction(nameof(GetSeatLockById), new { id = newId }, newId);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateSeatLock(Guid id, [FromBody] SeatLock seatLock)
    {
        if (!ModelState.IsValid)
        {
            logger.LogWarning("Admin: User - Model validation failed for updating seat lock ID {Id}: {ModelStateErrors}. Request: {Request}.",
                id, JsonSerializer.Serialize(ModelState), JsonSerializer.Serialize(seatLock));
            return BadRequest(ModelState);
        }

        if (id != seatLock.Id)
        {
            logger.LogWarning("Admin: User - Mismatch between ID from URL ({UrlId}) and payload ID ({PayloadId}) for updating seat lock.",
                id, seatLock.Id);
            return BadRequest(new ProblemDetails 
            { 
                Title = "ID Mismatch", 
                Detail = "ID in URL must match ID in request body.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        logger.LogInformation("Admin: User attempting to update seat lock with ID: {Id}. Data: {SeatLock}.", id, JsonSerializer.Serialize(seatLock));
        await adminSeatLockService.UpdateItem(id, seatLock);
        logger.LogInformation("Admin: User successfully updated seat lock with ID: {Id}.", id);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteSeatLock(Guid id)
    {
        logger.LogInformation("Admin: User attempting to delete seat lock with ID: {Id}.", id);
        await adminSeatLockService.DeleteItem(id);
        logger.LogInformation("Admin: User successfully deleted seat lock with ID: {Id}.", id);
        return NoContent();
    }
}