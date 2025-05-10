using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using railway_service.Models.Responces;
using RailwayApp.Application.Models;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces.IRepositories;
using RailwayApp.Domain.Interfaces.IServices;

namespace RailwayApp.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
//[Authorize]
[Authorize(Roles = "Client")]
public class StationsController(IStationService stationService
, ILogger<StationsController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetStations()
    {
        if (User?.Identity?.IsAuthenticated == true)
        {
            var claimsInfo = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
            logger.LogInformation("User Claims: {@Claims}", claimsInfo); // Логируем клеймы
            // Вы можете также вернуть их клиенту для наглядности, если хотите
            // return Ok(new { Message = "Authenticated", Claims = claimsInfo });
        }
        else
        {
            logger.LogWarning("User is not authenticated or User/Identity is null.");
            // return Unauthorized("User not authenticated for debug."); // Если IsAuthenticated == false
        }
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
                logger.LogWarning("Incorrect: {@Errors}", ModelState);
                return BadRequest(ModelState);
            }

            var station = await stationService.CreateStationAsync(request);
            return CreatedAtAction(nameof(GetStations), station);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Error creation station: {Message}", ex.Message);
            return Conflict(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Internal Server Error {Message}", ex.Message);
            return StatusCode(500, "Internal Server Error");
        }
    }
}