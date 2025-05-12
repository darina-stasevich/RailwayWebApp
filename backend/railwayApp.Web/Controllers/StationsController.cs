using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RailwayApp.Application.Models;
using RailwayApp.Domain.Interfaces.IServices;

namespace RailwayApp.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
//[Authorize]
[Authorize(Roles = "Client")]
public class StationsController(IStationService stationService, ILogger<StationsController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetStations()
    {
        var stations = await stationService.GetAllStationsAsync();
        return Ok(stations);
    }

    [HttpPost]
    public async Task<IActionResult> CreateStation([FromBody] CreateStationRequest request)
    {
        if (!ModelState.IsValid)
        {
            logger.LogWarning("Incorrect: {@Errors}", ModelState);
            return BadRequest(ModelState);
        }

        var station = await stationService.CreateStationAsync(request);
        return CreatedAtAction(nameof(GetStations), station);
    }
}