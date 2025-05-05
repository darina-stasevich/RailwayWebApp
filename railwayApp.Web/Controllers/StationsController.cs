using Microsoft.AspNetCore.Mvc;
using railway_service.Models.Responces;
using RailwayApp.Application.Models;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces.IRepositories;
using RailwayApp.Domain.Interfaces.IServices;

namespace RailwayApp.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StationsController(IStationService stationService
, ILogger<StationsController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetStations()
    {
        try
        {
            var stations = await stationService.GetAllStationsAsync();
            return Ok(stations);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка получения списка станций");
            return StatusCode(500, "Internal Server Error");
        }
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateStation([FromBody] CreateStationRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                logger.LogWarning("Некорректный запрос: {@Errors}", ModelState);
                return BadRequest(ModelState);
            }

            var station = await stationService.CreateStationAsync(request);
            return CreatedAtAction(nameof(GetStations), station);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Ошибка создания станции: {Message}", ex.Message);
            return Conflict(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка сервера при создании станции");
            return StatusCode(500, "Internal Server Error");
        }
    }
}