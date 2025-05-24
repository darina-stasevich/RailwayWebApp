using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using railway_service;
// Если GetAllStationsAsync возвращает, например, List<StationDto>, то нужно добавить using для StationDto
// using RailwayApp.Application.Models; 
using RailwayApp.Domain.Interfaces.IServices; // Для IStationService

namespace RailwayApp.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Client")] 
public class StationsController(
    IStationService stationService,
    ILogger<StationsController> logger,
    IMyCustomLogger customLogger)
    : ControllerBase
{
    private const string LoggerNameForCustomLog = "StationsController";

  
    [HttpGet]
    public async Task<IActionResult> GetStations()
    {
        logger.LogInformation("Attempting to get all stations.");

        customLogger.Info("Attempting to get all stations", LoggerNameForCustomLog,
            context: new Dictionary<string, object> { { "Action", "GetAllStations" } });

        var stations = await stationService.GetAllStationsAsync();

        int stationCount = 0;
        if (stations is System.Collections.ICollection collection)
        {
            stationCount = collection.Count;
        }
        else if (stations != null)
        {
            try
            {
                stationCount = Enumerable.Count(stations as dynamic); 
            }
            catch
            {
                stationCount = (stations != null ? 1 : 0); 
            }
        }

        logger.LogInformation("Successfully retrieved {StationCount} stations.", stationCount);

        customLogger.Info("Successfully retrieved stations", LoggerNameForCustomLog,
            context: new Dictionary<string, object>
            {
                { "StationCount", stationCount }
            });

        return Ok(stations);
    }
}