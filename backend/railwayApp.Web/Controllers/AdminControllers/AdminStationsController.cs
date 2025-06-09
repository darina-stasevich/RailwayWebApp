using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RailwayApp.Application.Models;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces.IServices;
using RailwayApp.Domain.Interfaces.IServices.AdminServices;

namespace RailwayApp.Web.Controllers.AdminControllers;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminStationsController(IAdminStationService adminStationService,
    ILogger<AdminStationsController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Station>>> GetAllStations()
    {
        logger.LogInformation("Admin: User attempting to retrieve all stations.");
        var items = await adminStationService.GetAllItems();
        logger.LogInformation("Admin: User successfully retrieved {Count} stations.", items.Count());
        return Ok(items);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Station>> GetStationById(Guid id)
    {
        logger.LogInformation("Admin: User attempting to retrieve station with ID: {Id}.", id);
        
        var station = await adminStationService.GetItemByIdAsync(id); 
        
        logger.LogInformation("Admin: User successfully retrieved station with ID: {Id}.", id);
        return Ok(station);
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> CreateStation([FromBody] Station station)
    {
        if (!ModelState.IsValid)
        {
            logger.LogWarning("Admin: User - Model validation failed for creating station: {ModelStateErrors}. Request: {Request}.", JsonSerializer.Serialize(ModelState), JsonSerializer.Serialize(station));
            return BadRequest(ModelState);
        }

        logger.LogInformation("Admin: User attempting to create station: {Station}.", JsonSerializer.Serialize(station));
        var newId = await adminStationService.CreateItem(station);
        logger.LogInformation("Admin: User successfully created station with ID: {Id}.", newId);
        return CreatedAtAction(nameof(GetStationById), new { id = newId }, newId);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateStation(Guid id, [FromBody] Station station)
    {
        if (!ModelState.IsValid)
        {
            logger.LogWarning("Admin: User - Model validation failed for updating station ID {Id}: {ModelStateErrors}. Request: {Request}.", id, JsonSerializer.Serialize(ModelState), JsonSerializer.Serialize(station));
            return BadRequest(ModelState);
        }

        if (id != station.Id)
        {
            logger.LogWarning("Admin: User - Mismatch between ID from URL ({UrlId}) and payload ID ({PayloadId}) for updating station.", id, station.Id);
            return BadRequest(new ProblemDetails 
            { 
                Title = "ID Mismatch", 
                Detail = "ID in URL must match ID in request body.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        logger.LogInformation("Admin: User attempting to update station with ID: {Id}. Data: {Station}.", id, JsonSerializer.Serialize(station));
        await adminStationService.UpdateItem(id, station);
        logger.LogInformation("Admin: User successfully updated station with ID: {Id}.", id);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteStation(Guid id)
    {
        logger.LogInformation("Admin: User attempting to delete station with ID: {Id}.", id);
        await adminStationService.DeleteItem(id);
        logger.LogInformation("Admin: User successfully deleted station with ID: {Id}.", id);
        return NoContent();
    }
}