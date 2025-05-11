using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RailwayApp.Application.Models;
using IAuthorizationService = RailwayApp.Domain.Interfaces.IServices.IAuthorizationService;

namespace RailwayApp.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LoginController(
    IAuthorizationService authorizationService,
    ILogger<LoginController> logger) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var loginResponse = await authorizationService.AuthorizeAsync(request);
        if (loginResponse == null)
            return Unauthorized(new { message = "Invalid email or password" });

        logger.LogInformation("successful authorization for user {email}", request.Email);
        return Ok(loginResponse);
    }
}