using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces.IServices.AdminServices;

namespace RailwayApp.Web.Controllers.AdminControllers;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminTrainsController(
    IAdminTrainService adminTrainService,
    ILogger<AdminTrainsController> logger)
    : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Train>>> GetAllTrains()
    {
        logger.LogInformation("Admin: User attempting to retrieve all trains.");
        var items = await adminTrainService.GetAllItems();
        logger.LogInformation("Admin: User successfully retrieved {Count} trains.", items.Count());
        return Ok(items);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Train>> GetTrainById(string id)
    {
        logger.LogInformation("Admin: User attempting to retrieve train with ID (Number): {Id}.", id);
        
        var train = await adminTrainService.GetItemByIdAsync(id); 
        
        logger.LogInformation("Admin: User successfully retrieved train with ID (Number): {Id}.", id);
        return Ok(train);
    }

    [HttpPost]
    public async Task<ActionResult<string>> CreateTrain([FromBody] Train train)
    {
        if (!ModelState.IsValid)
        {
            logger.LogWarning("Admin: User - Model validation failed for creating train: {ModelStateErrors}. Request: {Request}.", JsonSerializer.Serialize(ModelState), JsonSerializer.Serialize(train));
            return BadRequest(ModelState);
        }

        logger.LogInformation("Admin: User attempting to create train: {Train}.", JsonSerializer.Serialize(train));
        var newId = await adminTrainService.CreateItem(train);
        logger.LogInformation("Admin: User successfully created train with ID (Number): {Id}.", newId);
        return CreatedAtAction(nameof(GetTrainById), new { id = newId }, newId);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTrain(string id, [FromBody] Train train)
    {
        if (!ModelState.IsValid)
        {
            logger.LogWarning("Admin: User - Model validation failed for updating train ID (Number) {Id}: {ModelStateErrors}. Request: {Request}.", id, JsonSerializer.Serialize(ModelState), JsonSerializer.Serialize(train));
            return BadRequest(ModelState);
        }

        if (id != train.Id)
        {
            logger.LogWarning("Admin: User - Mismatch between ID (Number) from URL ({UrlId}) and payload ID ({PayloadId}) for updating train.", id, train.Id);
            return BadRequest(new ProblemDetails 
            { 
                Title = "ID Mismatch", 
                Detail = "ID (Train Number) in URL must match ID in request body.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        logger.LogInformation("Admin: User attempting to update train with ID (Number): {Id}. Data: {Train}.", id, JsonSerializer.Serialize(train)); await adminTrainService.UpdateItem(id, train);
        logger.LogInformation("Admin: User successfully updated train with ID (Number): {Id}.", id);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTrain(string id)
    {
        logger.LogInformation("Admin: User attempting to delete train with ID (Number): {Id}.", id);
        await adminTrainService.DeleteItem(id);
        logger.LogInformation("Admin: User successfully deleted train with ID (Number): {Id}.", id);
        return NoContent();
    }
}