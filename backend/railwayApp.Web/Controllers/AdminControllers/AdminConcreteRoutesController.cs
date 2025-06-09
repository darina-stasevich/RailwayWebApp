using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces.IServices.AdminServices;

namespace RailwayApp.Web.Controllers.AdminControllers;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminConcreteRoutesController(
    IAdminConcreteRouteService adminConcreteRouteService,
    ILogger<AdminConcreteRoutesController> logger)
    : ControllerBase
{
    private string GetCurrentTimestamp() => DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ConcreteRoute>>> GetAllConcreteRoutes()
    {
        logger.LogInformation("Admin: User attempting to retrieve all concrete routes. Timestamp: {TimestampUTC}", GetCurrentTimestamp());
        var items = await adminConcreteRouteService.GetAllItems();
        logger.LogInformation("Admin: User  successfully retrieved {Count} concrete routes. Timestamp: {TimestampUTC}", items.Count(), GetCurrentTimestamp());
        return Ok(items);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ConcreteRoute>> GetConcreteRouteById(Guid id)
    {
        logger.LogInformation("Admin: User  attempting to retrieve concrete route with ID: {Id}. Timestamp: {TimestampUTC}", id, GetCurrentTimestamp());
        
        var concreteRoute = await adminConcreteRouteService.GetItemByIdAsync(id); 
        
        logger.LogInformation("Admin: User  successfully retrieved concrete route with ID: {Id}. Timestamp: {TimestampUTC}", id, GetCurrentTimestamp());
        return Ok(concreteRoute);
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> CreateConcreteRoute([FromBody] ConcreteRoute concreteRoute)
    {
        if (!ModelState.IsValid)
        {
            logger.LogWarning("Admin: User  - Model validation failed for creating concrete route: {ModelStateErrors}. Request: {Request}. Timestamp: {TimestampUTC}",
                JsonSerializer.Serialize(ModelState), JsonSerializer.Serialize(concreteRoute), GetCurrentTimestamp());
            return BadRequest(ModelState);
        }

        logger.LogInformation("Admin: User  attempting to create concrete route: {ConcreteRoute}. Timestamp: {TimestampUTC}", JsonSerializer.Serialize(concreteRoute), GetCurrentTimestamp());
        var newId = await adminConcreteRouteService.CreateItem(concreteRoute);
        logger.LogInformation("Admin: User  successfully created concrete route with ID: {Id}. Timestamp: {TimestampUTC}", newId, GetCurrentTimestamp());
        return CreatedAtAction(nameof(GetConcreteRouteById), new { id = newId }, newId);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateConcreteRoute(Guid id, [FromBody] ConcreteRoute concreteRoute)
    {
        if (!ModelState.IsValid)
        {
            logger.LogWarning("Admin: User  - Model validation failed for updating concrete route ID {Id}: {ModelStateErrors}. Request: {Request}. Timestamp: {TimestampUTC}",
                id, JsonSerializer.Serialize(ModelState), JsonSerializer.Serialize(concreteRoute), GetCurrentTimestamp());
            return BadRequest(ModelState);
        }

        if (id != concreteRoute.Id)
        {
            logger.LogWarning("Admin: User  - Mismatch between ID from URL ({UrlId}) and payload ID ({PayloadId}) for updating concrete route. Timestamp: {TimestampUTC}",
                id, concreteRoute.Id, GetCurrentTimestamp());
            return BadRequest(new ProblemDetails 
            { 
                Title = "ID Mismatch", 
                Detail = "ID in URL must match ID in request body.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        logger.LogInformation("Admin: User  attempting to update concrete route with ID: {Id}. Data: {ConcreteRoute}. Timestamp: {TimestampUTC}", id, JsonSerializer.Serialize(concreteRoute), GetCurrentTimestamp());
        await adminConcreteRouteService.UpdateItem(id, concreteRoute);
        logger.LogInformation("Admin: User  successfully updated concrete route with ID: {Id}. Timestamp: {TimestampUTC}", id, GetCurrentTimestamp());
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteConcreteRoute(Guid id)
    {
        logger.LogInformation("Admin: User  attempting to delete concrete route with ID: {Id}. Timestamp: {TimestampUTC}", id, GetCurrentTimestamp());
        await adminConcreteRouteService.DeleteItem(id);
        logger.LogInformation("Admin: User  successfully deleted concrete route with ID: {Id}. Timestamp: {TimestampUTC}", id, GetCurrentTimestamp());
        return NoContent();
    }
}