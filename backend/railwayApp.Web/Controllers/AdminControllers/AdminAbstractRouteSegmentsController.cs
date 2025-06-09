using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces.IServices.AdminServices;

namespace RailwayApp.Web.Controllers.AdminControllers;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminAbstractRouteSegmentsController(
    IAdminAbstractRouteSegmentService adminAbstractRouteSegmentService,
    ILogger<AdminAbstractRouteSegmentsController> logger)
    : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AbstractRouteSegment>>> GetAllAbstractRouteSegments()
    {
        logger.LogInformation("Admin: Attempting to retrieve all abstract route segments.");
        var items = await adminAbstractRouteSegmentService.GetAllItems();
        logger.LogInformation("Admin: Successfully retrieved {Count} abstract route segments.", items.Count());
        return Ok(items);
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> CreateAbstractRouteSegment([FromBody] AbstractRouteSegment abstractRouteSegment)
    {
        if (!ModelState.IsValid)
        {
            logger.LogWarning("Admin: Model validation failed for creating abstract route segment: {ModelStateErrors}. Request: {Request}",
                JsonSerializer.Serialize(ModelState), JsonSerializer.Serialize(abstractRouteSegment));
            return BadRequest(ModelState);
        }

        logger.LogInformation("Admin: Attempting to create abstract route segment: {AbstractRouteSegment}", JsonSerializer.Serialize(abstractRouteSegment));
        var newId = await adminAbstractRouteSegmentService.CreateItem(abstractRouteSegment);
        logger.LogInformation("Admin: Successfully created abstract route segment with ID: {Id}", newId);
        return CreatedAtAction(nameof(GetAbstractRouteSegmentById), new { id = newId }, newId);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AbstractRouteSegment>> GetAbstractRouteSegmentById(Guid id)
    {
        logger.LogInformation("Admin: Attempting to retrieve abstract route segment with ID: {Id}", id);
        var segment = await adminAbstractRouteSegmentService.GetItemByIdAsync(id); 
        if (segment == null)
        {
             logger.LogWarning("Admin: Abstract route segment with ID: {Id} not found by GetAbstractRouteSegmentById after GetAllItems call.", id);
            return NotFound(new { message = $"AbstractRouteSegment with ID {id} not found." });
        }
        logger.LogInformation("Admin: Successfully retrieved abstract route segment with ID: {Id}", id);
        return Ok(segment);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateAbstractRouteSegment(Guid id, [FromBody] AbstractRouteSegment abstractRouteSegment)
    {
        if (!ModelState.IsValid)
        {
            logger.LogWarning("Admin: Model validation failed for updating abstract route segment ID {Id}: {ModelStateErrors}. Request: {Request}",
                id, JsonSerializer.Serialize(ModelState), JsonSerializer.Serialize(abstractRouteSegment));
            return BadRequest(ModelState);
        }

        if (id != abstractRouteSegment.Id)
        {
            logger.LogWarning("Admin: Mismatch between route ID ({RouteId}) and payload ID ({PayloadId}) for updating abstract route segment.",
                id, abstractRouteSegment.Id);
            return BadRequest(new { message = "ID in URL must match ID in request body." });
        }

        logger.LogInformation("Admin: Attempting to update abstract route segment with ID: {Id}. Data: {AbstractRouteSegment}", id, JsonSerializer.Serialize(abstractRouteSegment));
        await adminAbstractRouteSegmentService.UpdateItem(id, abstractRouteSegment);
        logger.LogInformation("Admin: Successfully updated abstract route segment with ID: {Id}", id);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAbstractRouteSegment(Guid id)
    {
        logger.LogInformation("Admin: Attempting to delete abstract route segment with ID: {Id}", id);
        await adminAbstractRouteSegmentService.DeleteItem(id);
        logger.LogInformation("Admin: Successfully deleted abstract route segment with ID: {Id}", id);
        return NoContent();
    }
}