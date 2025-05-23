using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using railway_service;
using RailwayApp.Domain.Interfaces.IServices; // Для IScheduleService
// Предполагается, что IMyCustomLogger и его реализация (MyCustomFileJsonLogger) определены
// и зарегистрированы в DI.
// Например, в Program.cs:
// builder.Services.AddSingleton<IMyCustomLogger, MyCustomFileJsonLogger>();

// Предполагается, что IScheduleService.GetScheduleAsync возвращает что-то вроде List<ScheduleDto> или подобное.
// Для примера, я буду считать, что он возвращает объект, который можно передать в Ok().

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
        // Стандартное логирование (Serilog) - перед вызовом сервиса
        logger.LogInformation("Attempting to get schedule for ConcreteRouteId: {ConcreteRouteId}", concreteRouteId);

        // Твое кастомное логирование - перед вызовом сервиса
        customLogger.Info("Attempting to get schedule", LoggerNameForCustomLog,
            context: new Dictionary<string, object>
            {
                { "ConcreteRouteId", concreteRouteId }
            });

        var schedules = await scheduleService.GetScheduleAsync(concreteRouteId);

        if (schedules == null) // Или schedules.Count == 0, в зависимости от того, что возвращает сервис
        {
            // Стандартное логирование (Serilog) - если расписание не найдено
            logger.LogWarning("Schedule not found for ConcreteRouteId: {ConcreteRouteId}", concreteRouteId);
            // Твое кастомное логирование - если расписание не найдено
            customLogger.Warn("Schedule not found", LoggerNameForCustomLog,
                exception: null,
                context: new Dictionary<string, object> { { "ConcreteRouteId", concreteRouteId } });
            return NotFound($"Schedule not found for route ID {concreteRouteId}.");
        }

        // Стандартное логирование (Serilog) - после успешного вызова сервиса
        // Используется LogWarning как в твоем оригинальном коде. Если это успех, лучше LogInformation.
        logger.LogWarning("Schedule received for route {ConcreteRouteId}. Item count: {ItemCount}",
            concreteRouteId,
            (schedules as System.Collections.ICollection)?.Count ?? (schedules != null ? 1 : 0) // Примерное определение количества
        );


        // Твое кастомное логирование - после успешного вызова сервиса
        // Используется Warn как в твоем оригинальном коде. Если это успех, лучше Info.
        customLogger.Warn("Schedule received", LoggerNameForCustomLog, // Используем Warn, так как в оригинале был LogWarning
            exception: null, // Если это не ошибка, то exception: null
            context: new Dictionary<string, object>
            {
                { "ConcreteRouteId", concreteRouteId },
                // Можно добавить количество элементов или сами данные, если они не слишком большие
                { "ScheduleItemCount", (schedules as System.Collections.ICollection)?.Count ?? (schedules != null ? 1 : 0) },
                // { "ReceivedSchedules", schedules } // Раскомментируй, если нужно логировать сами данные
            });

        return Ok(schedules);
    }
}