using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces.IServices.AdminServices;

namespace RailwayApp.Web.Controllers.AdminControllers;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminConcreteRouteSegmentsController(
    IAdminConcreteRouteSegmentService adminConcreteRouteSegmentService,
    ILogger<AdminConcreteRouteSegmentsController> logger)
    : ControllerBase
{
    private string GetCurrentTimestamp() => DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ConcreteRouteSegment>>> GetAllConcreteRouteSegments()
    {
        logger.LogInformation("Admin: attempting to retrieve all concrete route segments. Timestamp: {TimestampUTC}", GetCurrentTimestamp());
        var items = await adminConcreteRouteSegmentService.GetAllItems();
        logger.LogInformation("Admin: User successfully retrieved {Count} concrete route segments. Timestamp: {TimestampUTC}", items.Count(), GetCurrentTimestamp());
        return Ok(items);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ConcreteRouteSegment>> GetConcreteRouteSegmentById(Guid id)
    {
        logger.LogInformation("Admin: User attempting to retrieve concrete route segment with ID: {Id}. Timestamp: {TimestampUTC}", id, GetCurrentTimestamp());
        var concreteRouteSegment = await adminConcreteRouteSegmentService.GetItemByIdAsync(id); 
        
        logger.LogInformation("Admin: User successfully retrieved concrete route segment with ID: {Id}. Timestamp: {TimestampUTC}", id, GetCurrentTimestamp());
        return Ok(concreteRouteSegment);
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> CreateConcreteRouteSegment([FromBody] ConcreteRouteSegment concreteRouteSegment)
    {
        if (!ModelState.IsValid)
        {
            logger.LogWarning("Admin: User - Model validation failed for creating concrete route segment: {ModelStateErrors}. Request: {Request}. Timestamp: {TimestampUTC}", JsonSerializer.Serialize(ModelState), JsonSerializer.Serialize(concreteRouteSegment), GetCurrentTimestamp());
            return BadRequest(ModelState);
        }

        logger.LogInformation("Admin: User attempting to create concrete route segment: {ConcreteRouteSegment}. Timestamp: {TimestampUTC}", JsonSerializer.Serialize(concreteRouteSegment), GetCurrentTimestamp());
        var newId = await adminConcreteRouteSegmentService.CreateItem(concreteRouteSegment);
        logger.LogInformation("Admin: User successfully created concrete route segment with ID: {Id}. Timestamp: {TimestampUTC}", newId, GetCurrentTimestamp());
        return CreatedAtAction(nameof(GetConcreteRouteSegmentById), new { id = newId }, newId);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateConcreteRouteSegment(Guid id, [FromBody] ConcreteRouteSegment concreteRouteSegment)
    {
        if (!ModelState.IsValid)
        {
            logger.LogWarning("Admin: User - Model validation failed for updating concrete route segment ID {Id}: {ModelStateErrors}. Request: {Request}. Timestamp: {TimestampUTC}", id, JsonSerializer.Serialize(ModelState), JsonSerializer.Serialize(concreteRouteSegment), GetCurrentTimestamp());
            return BadRequest(ModelState);
        }

        if (id != concreteRouteSegment.Id)
        {
            logger.LogWarning("Admin: User - Mismatch between ID from URL ({UrlId}) and payload ID ({PayloadId}) for updating concrete route segment. Timestamp: {TimestampUTC}", id, concreteRouteSegment.Id, GetCurrentTimestamp());
            return BadRequest(new ProblemDetails 
            { 
                Title = "ID Mismatch", 
                Detail = "ID in URL must match ID in request body.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        logger.LogInformation("Admin: User attempting to update concrete route segment with ID: {Id}. Data: {ConcreteRouteSegment}. Timestamp: {TimestampUTC}", id, JsonSerializer.Serialize(concreteRouteSegment), GetCurrentTimestamp());
        await adminConcreteRouteSegmentService.UpdateItem(id, concreteRouteSegment);
        logger.LogInformation("Admin: User successfully updated concrete route segment with ID: {Id}. Timestamp: {TimestampUTC}", id, GetCurrentTimestamp());
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteConcreteRouteSegment(Guid id)
    {
        logger.LogInformation("Admin: User attempting to delete concrete route segment with ID: {Id}. Timestamp: {TimestampUTC}", id, GetCurrentTimestamp());
        await adminConcreteRouteSegmentService.DeleteItem(id);
        logger.LogInformation("Admin: User successfully deleted concrete route segment with ID: {Id}. Timestamp: {TimestampUTC}", id, GetCurrentTimestamp());
        return NoContent();
    }
}