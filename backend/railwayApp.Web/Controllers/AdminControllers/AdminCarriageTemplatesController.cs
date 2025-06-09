using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces.IServices.AdminServices;

namespace RailwayApp.Web.Controllers.AdminControllers;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminCarriageTemplatesController(
    IAdminCarriageTemplateService adminCarriageTemplateService,
    ILogger<AdminCarriageTemplatesController> logger)
    : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CarriageTemplate>>> GetAllCarriageTemplates()
    {
        var userLogin = User.Identity?.Name ?? "darina-stasevich";
        logger.LogInformation("Admin: User {User} attempting to retrieve all carriage templates. Timestamp: {TimestampUTC}", userLogin, DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
        var items = await adminCarriageTemplateService.GetAllItems();
        logger.LogInformation("Admin: User {User} successfully retrieved {Count} carriage templates. Timestamp: {TimestampUTC}", userLogin, items.Count(), DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
        return Ok(items);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CarriageTemplate>> GetCarriageTemplateById(Guid id)
    {
        var userLogin = User.Identity?.Name ?? "darina-stasevich";
        logger.LogInformation("Admin: User {User} attempting to retrieve carriage template with ID: {Id}. Timestamp: {TimestampUTC}", userLogin, id, DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
        
        var carriageTemplate = await adminCarriageTemplateService.GetItemByIdAsync(id); 
        
        logger.LogInformation("Admin: User {User} successfully retrieved carriage template with ID: {Id}. Timestamp: {TimestampUTC}", userLogin, id, DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
        return Ok(carriageTemplate);
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> CreateCarriageTemplate([FromBody] CarriageTemplate carriageTemplate)
    {
        var userLogin = User.Identity?.Name ?? "darina-stasevich";
        if (!ModelState.IsValid)
        {
            logger.LogWarning("Admin: User {User} - Model validation failed for creating carriage template: {ModelStateErrors}. Request: {Request}. Timestamp: {TimestampUTC}",
                userLogin, JsonSerializer.Serialize(ModelState), JsonSerializer.Serialize(carriageTemplate), DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
            return BadRequest(ModelState);
        }

        logger.LogInformation("Admin: User {User} attempting to create carriage template: {CarriageTemplate}. Timestamp: {TimestampUTC}", userLogin, JsonSerializer.Serialize(carriageTemplate), DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
        var newId = await adminCarriageTemplateService.CreateItem(carriageTemplate);
        logger.LogInformation("Admin: User {User} successfully created carriage template with ID: {Id}. Timestamp: {TimestampUTC}", userLogin, newId, DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
        return CreatedAtAction(nameof(GetCarriageTemplateById), new { id = newId }, newId);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateCarriageTemplate(Guid id, [FromBody] CarriageTemplate carriageTemplate)
    {
        var userLogin = User.Identity?.Name ?? "darina-stasevich";
        if (!ModelState.IsValid)
        {
            logger.LogWarning("Admin: User {User} - Model validation failed for updating carriage template ID {Id}: {ModelStateErrors}. Request: {Request}. Timestamp: {TimestampUTC}",
                userLogin, id, JsonSerializer.Serialize(ModelState), JsonSerializer.Serialize(carriageTemplate), DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
            return BadRequest(ModelState);
        }

        if (id != carriageTemplate.Id)
        {
            logger.LogWarning("Admin: User {User} - Mismatch between ID from URL ({UrlId}) and payload ID ({PayloadId}) for updating carriage template. Timestamp: {TimestampUTC}",
                userLogin, id, carriageTemplate.Id, DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
            return BadRequest(new ProblemDetails 
            { 
                Title = "ID Mismatch", 
                Detail = "ID in URL must match ID in request body.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        logger.LogInformation("Admin: User {User} attempting to update carriage template with ID: {Id}. Data: {CarriageTemplate}. Timestamp: {TimestampUTC}", userLogin, id, JsonSerializer.Serialize(carriageTemplate), DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
        await adminCarriageTemplateService.UpdateItem(id, carriageTemplate);
        logger.LogInformation("Admin: User {User} successfully updated carriage template with ID: {Id}. Timestamp: {TimestampUTC}", userLogin, id, DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteCarriageTemplate(Guid id)
    {
        var userLogin = User.Identity?.Name ?? "darina-stasevich";
        logger.LogInformation("Admin: User {User} attempting to delete carriage template with ID: {Id}. Timestamp: {TimestampUTC}", userLogin, id, DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
        await adminCarriageTemplateService.DeleteItem(id);
        logger.LogInformation("Admin: User {User} successfully deleted carriage template with ID: {Id}. Timestamp: {TimestampUTC}", userLogin, id, DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
        return NoContent();
    }
}