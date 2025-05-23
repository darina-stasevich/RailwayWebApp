using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using railway_service;
// Если GetAllStationsAsync возвращает, например, List<StationDto>, то нужно добавить using для StationDto
// using RailwayApp.Application.Models; 
using RailwayApp.Domain.Interfaces.IServices; // Для IStationService
// Предполагается, что IMyCustomLogger и его реализация (MyCustomFileJsonLogger) определены
// и зарегистрированы в DI.
// Например, в Program.cs:
// builder.Services.AddSingleton<IMyCustomLogger, MyCustomFileJsonLogger>();

namespace RailwayApp.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Client")] // Хотя для получения списка всех станций авторизация может быть и не нужна, или нужна другая роль
public class StationsController(
    IStationService stationService,
    ILogger<StationsController> logger,
    IMyCustomLogger customLogger)
    : ControllerBase
{
    private const string LoggerNameForCustomLog = "StationsController"; // Имя для кастомного логгера

    // Инъекция твоего кастомного логгера

    [HttpGet]
    public async Task<IActionResult> GetStations()
    {
        // Стандартное логирование (Serilog) - перед вызовом сервиса
        logger.LogInformation("Attempting to get all stations.");

        // Твое кастомное логирование - перед вызовом сервиса
        customLogger.Info("Attempting to get all stations", LoggerNameForCustomLog,
            context: new Dictionary<string, object> { { "Action", "GetAllStations" } });

        var stations = await stationService.GetAllStationsAsync();

        // Определяем количество станций для логирования
        int stationCount = 0;
        if (stations is System.Collections.ICollection collection)
        {
            stationCount = collection.Count;
        }
        else if (stations != null)
        {
            // Если это IEnumerable, но не ICollection, можно посчитать через Linq,
            // но это может привести к повторному перечислению, если stations - это "ленивая" последовательность.
            // Для простого списка это нормально.
            // Если stations может быть null, то Count() вызовет исключение.
            // Лучше проверить на null перед использованием Count().
            // var enumerableStations = stations as IEnumerable<object>; // Замените object на ваш тип станции
            // if (enumerableStations != null) stationCount = enumerableStations.Count();
            // Более безопасный вариант, если неизвестен точный тип, но может быть неэффективно:
            try
            {
                stationCount = Enumerable.Count(stations as dynamic); // Динамический подсчет, если это IEnumerable<T>
            }
            catch
            {
                stationCount = (stations != null ? 1 : 0); // Если не удалось посчитать, предполагаем 1 или 0
            }
        }


        // Стандартное логирование (Serilog) - после успешного вызова сервиса
        logger.LogInformation("Successfully retrieved {StationCount} stations.", stationCount);

        // Твое кастомное логирование - после успешного вызова сервиса
        customLogger.Info("Successfully retrieved stations", LoggerNameForCustomLog,
            context: new Dictionary<string, object>
            {
                { "StationCount", stationCount }
                // Можно добавить сами станции, если их немного и это полезно для лога,
                // но для "всех станций" это может быть слишком много данных.
                // { "RetrievedStations", stations } // Раскомментируй с осторожностью
            });

        return Ok(stations);
    }
}