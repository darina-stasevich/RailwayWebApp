using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces.IServices.AdminServices;

namespace RailwayApp.Web.Controllers.AdminControllers;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminAbstractRoutesController(
    IAdminAbstractRouteService adminAbstractRouteService,
    ILogger<AdminAbstractRoutesController> logger)
    : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AbstractRoute>>> GetAllAbstractRoutes()
    {
        var userLogin = User.Identity?.Name ?? "Unknown";
        logger.LogInformation("Admin: Attempting to retrieve all abstract routes. User: {User}", userLogin);
        var items = await adminAbstractRouteService.GetAllItems();
        logger.LogInformation("Admin: Successfully retrieved {Count} abstract routes. User: {User}", items.Count(), userLogin);
        return Ok(items);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AbstractRoute>> GetAbstractRouteById(Guid id)
    {
        var userLogin = User.Identity?.Name ?? "Unknown";
        logger.LogInformation("Admin: Attempting to retrieve abstract route with ID: {Id}. User: {User}", id, userLogin);
        
        var route = await adminAbstractRouteService.GetItemByIdAsync(id); 
        
        logger.LogInformation("Admin: Successfully retrieved abstract route with ID: {Id}. User: {User}", id, userLogin);
        return Ok(route);
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> CreateAbstractRoute([FromBody] AbstractRoute abstractRoute)
    {
        var userLogin = User.Identity?.Name ?? "Unknown";
        if (!ModelState.IsValid)
        {
            logger.LogWarning("Admin: Model validation failed for creating abstract route: {ModelStateErrors}. Request: {Request}. User: {User}",
                JsonSerializer.Serialize(ModelState), JsonSerializer.Serialize(abstractRoute), userLogin);
            return BadRequest(ModelState);
        }

        logger.LogInformation("Admin: Attempting to create abstract route: {AbstractRoute}. User: {User}", JsonSerializer.Serialize(abstractRoute), userLogin);
        var newId = await adminAbstractRouteService.CreateItem(abstractRoute);
        logger.LogInformation("Admin: Successfully created abstract route with ID: {Id}. User: {User}", newId, userLogin);
        return CreatedAtAction(nameof(GetAbstractRouteById), new { id = newId }, newId);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateAbstractRoute(Guid id, [FromBody] AbstractRoute abstractRoute)
    {
        var userLogin = User.Identity?.Name ?? "Unknown";
        if (!ModelState.IsValid)
        {
            logger.LogWarning("Admin: Model validation failed for updating abstract route ID {Id}: {ModelStateErrors}. Request: {Request}. User: {User}",
                id, JsonSerializer.Serialize(ModelState), JsonSerializer.Serialize(abstractRoute), userLogin);
            return BadRequest(ModelState);
        }

        if (id != abstractRoute.Id)
        {
            logger.LogWarning("Admin: Mismatch between route ID from URL ({RouteIdFromUrl}) and payload ID ({RouteIdFromPayload}) for updating abstract route. User: {User}",
                id, abstractRoute.Id, userLogin);
            return BadRequest(new ProblemDetails 
            { 
                Title = "ID Mismatch", 
                Detail = "ID in URL must match ID in request body.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        logger.LogInformation("Admin: Attempting to update abstract route with ID: {Id}. Data: {AbstractRoute}. User: {User}", id, JsonSerializer.Serialize(abstractRoute), userLogin);
        await adminAbstractRouteService.UpdateItem(id, abstractRoute);
        logger.LogInformation("Admin: Successfully updated abstract route with ID: {Id}. User: {User}", id, userLogin);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAbstractRoute(Guid id)
    {
        var userLogin = User.Identity?.Name ?? "Unknown";
        logger.LogInformation("Admin: Attempting to delete abstract route with ID: {Id}. User: {User}", id, userLogin);
        await adminAbstractRouteService.DeleteItem(id);
        logger.LogInformation("Admin: Successfully deleted abstract route with ID: {Id}. User: {User}", id, userLogin);
        return NoContent();
    }
}