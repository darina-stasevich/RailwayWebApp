using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RailwayApp.Application.Models;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces.IServices;
using RailwayApp.Domain.Interfaces.IServices.AdminServices;

namespace RailwayApp.Web.Controllers.AdminControllers;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Roles = "Admin")]
public class StationsController(IAdminStationService adminStationService,
    ILogger<StationsController> logger) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateStation([FromBody] Station request)
    {
        if (!ModelState.IsValid)
        {
            logger.LogWarning("Incorrect: {@Errors}", ModelState);
            return BadRequest(ModelState);
        }

        var stationId = await adminStationService.CreateItem(request);
        return Ok(stationId);
    }
}