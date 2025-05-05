using Microsoft.AspNetCore.Mvc;
using RailwayApp.Domain.Interfaces.IServices;

namespace RailwayApp.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SchedulesController(ILogger<SchedulesController> logger,
    IScheduleService scheduleService) : ControllerBase
{
    [HttpGet("{concreteRouteId:guid}")]
    public async Task<IActionResult> GetSchedules([FromRoute] Guid concreteRouteId)
    {
        try
        {
            var schedules = await scheduleService.GetScheduleAsync(concreteRouteId);
            logger.LogWarning("Schedule received for route {ConcreteRouteId}", concreteRouteId);
            return Ok(schedules);
        }
        catch (ArgumentException ex)
        {
            logger.LogInformation(ex, "No schedule found for route {ConcreteRouteId} or data is invalid", concreteRouteId);
            return NotFound($"Schedule not found for route ID: {concreteRouteId}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting schedule for route {concreteRouteId}. {Message}", concreteRouteId, ex.Message);
            return StatusCode(500, "Internal Server Error");
        }
    }
}