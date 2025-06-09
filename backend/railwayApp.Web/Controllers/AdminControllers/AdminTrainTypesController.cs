using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces.IServices.AdminServices;

namespace RailwayApp.Web.Controllers.AdminControllers;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminTrainTypesController(
    IAdminTrainTypeService adminTrainTypeService,
    ILogger<AdminTrainTypesController> logger)
    : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TrainType>>> GetAllTrainTypes()
    {
        logger.LogInformation("Admin: User attempting to retrieve all train types.");
        var items = await adminTrainTypeService.GetAllItems();
        logger.LogInformation("Admin: User successfully retrieved {Count} train types.", items.Count());
        return Ok(items);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TrainType>> GetTrainTypeById(Guid id)
    {
        logger.LogInformation("Admin: User attempting to retrieve train type with ID: {Id}.", id);
        
        var trainType = await adminTrainTypeService.GetItemByIdAsync(id); 
        
        logger.LogInformation("Admin: User successfully retrieved train type with ID: {Id}.", id);
        return Ok(trainType);
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> CreateTrainType([FromBody] TrainType trainType)
    {
        if (!ModelState.IsValid)
        {
            logger.LogWarning("Admin: User - Model validation failed for creating train type: {ModelStateErrors}. Request: {Request}.",
                JsonSerializer.Serialize(ModelState), JsonSerializer.Serialize(trainType));
            return BadRequest(ModelState);
        }

        logger.LogInformation("Admin: User attempting to create train type: {TrainType}.", JsonSerializer.Serialize(trainType));
        var newId = await adminTrainTypeService.CreateItem(trainType);
        logger.LogInformation("Admin: User successfully created train type with ID: {Id}.", newId);
        return CreatedAtAction(nameof(GetTrainTypeById), new { id = newId }, newId);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateTrainType(Guid id, [FromBody] TrainType trainType)
    {
        if (!ModelState.IsValid)
        {
            logger.LogWarning("Admin: User - Model validation failed for updating train type ID {Id}: {ModelStateErrors}. Request: {Request}.",
                id, JsonSerializer.Serialize(ModelState), JsonSerializer.Serialize(trainType));
            return BadRequest(ModelState);
        }

        if (id != trainType.Id)
        {
            logger.LogWarning("Admin: User - Mismatch between ID from URL ({UrlId}) and payload ID ({PayloadId}) for updating train type.",
                id, trainType.Id);
            return BadRequest(new ProblemDetails 
            { 
                Title = "ID Mismatch", 
                Detail = "ID in URL must match ID in request body.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        logger.LogInformation("Admin: User attempting to update train type with ID: {Id}. Data: {TrainType}.", id, JsonSerializer.Serialize(trainType));
        await adminTrainTypeService.UpdateItem(id, trainType);
        logger.LogInformation("Admin: User successfully updated train type with ID: {Id}.", id);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteTrainType(Guid id)
    {
        logger.LogInformation("Admin: User attempting to delete train type with ID: {Id}.", id);
        await adminTrainTypeService.DeleteItem(id);
        logger.LogInformation("Admin: User successfully deleted train type with ID: {Id}.", id);
        return NoContent();
    }  
}