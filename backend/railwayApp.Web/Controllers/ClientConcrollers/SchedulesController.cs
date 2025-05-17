using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RailwayApp.Domain.Interfaces.IServices;

namespace RailwayApp.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Client")]
public class SchedulesController(
    ILogger<SchedulesController> logger,
    IScheduleService scheduleService) : ControllerBase
{
    [HttpGet("{concreteRouteId:guid}")]
    public async Task<IActionResult> GetSchedules([FromRoute] Guid concreteRouteId)
    {
        var schedules = await scheduleService.GetScheduleAsync(concreteRouteId);
        logger.LogWarning("Schedule received for route {ConcreteRouteId}", concreteRouteId);
        return Ok(schedules);
    }
}