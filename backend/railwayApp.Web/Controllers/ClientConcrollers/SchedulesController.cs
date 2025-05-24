using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using railway_service;
using RailwayApp.Domain.Interfaces.IServices; 

namespace RailwayApp.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Client")]
public class SchedulesController(
    ILogger<SchedulesController> logger,
    IScheduleService scheduleService,
    IMyCustomLogger customLogger)
    : ControllerBase
{
    private const string LoggerNameForCustomLog = "SchedulesController";
    
    [HttpGet("{concreteRouteId:guid}")]
    public async Task<IActionResult> GetSchedules([FromRoute] Guid concreteRouteId)
    {
        logger.LogInformation("Attempting to get schedule for ConcreteRouteId: {ConcreteRouteId}", concreteRouteId);

        customLogger.Info("Attempting to get schedule", LoggerNameForCustomLog,
            context: new Dictionary<string, object>
            {
                { "ConcreteRouteId", concreteRouteId }
            });

        var schedules = await scheduleService.GetScheduleAsync(concreteRouteId);

        if (schedules == null) 
        {
            logger.LogWarning("Schedule not found for ConcreteRouteId: {ConcreteRouteId}", concreteRouteId);
            customLogger.Warn("Schedule not found", LoggerNameForCustomLog,
                exception: null,
                context: new Dictionary<string, object> { { "ConcreteRouteId", concreteRouteId } });
            return NotFound($"Schedule not found for route ID {concreteRouteId}.");
        }

        logger.LogWarning("Schedule received for route {ConcreteRouteId}. Item count: {ItemCount}",
            concreteRouteId,
            (schedules as System.Collections.ICollection)?.Count ?? (schedules != null ? 1 : 0) // Примерное определение количества
        );


        customLogger.Warn("Schedule received", LoggerNameForCustomLog,
            exception: null, 
            context: new Dictionary<string, object>
            {
                { "ConcreteRouteId", concreteRouteId },
                { "ScheduleItemCount", (schedules as System.Collections.ICollection)?.Count ?? (schedules != null ? 1 : 0) },
            });

        return Ok(schedules);
    }
}