using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using railway_service;
using RailwayApp.Application.Models;
using IAuthorizationService = RailwayApp.Domain.Interfaces.IServices.IAuthorizationService; 

namespace RailwayApp.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LoginController(
    IAuthorizationService authorizationService,
    ILogger<LoginController> logger,
    IMyCustomLogger customLogger)
    : ControllerBase
{
   
    private const string LoggerNameForCustomLog = "LoginController";

    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            logger.LogWarning("Model validation failed for login request. Email: {Email}, Errors: {ModelStateErrors}",
                request?.Email, // Может быть null, если сам request null, но ModelState не будет IsValid
                System.Text.Json.JsonSerializer.Serialize(ModelState));

            var modelStateErrors = ModelState.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
            );
            customLogger.Warn("Model validation failed for login request", LoggerNameForCustomLog,
                exception: null,
                context: new Dictionary<string, object>
                {
                    { "ModelStateErrors", modelStateErrors },
                    { "AttemptedEmail", request?.Email }, 
                    { "FailedRequestData", request }
                });
            return BadRequest(ModelState);
        }

        var loginResponse = await authorizationService.AuthorizeAsync(request);

        if (loginResponse == null)
        {
            logger.LogWarning("Authorization failed for user {Email}. Invalid email or password.", request.Email);

            customLogger.Warn("Authorization failed", LoggerNameForCustomLog,
                exception: null,
                context: new Dictionary<string, object>
                {
                    { "Email", request.Email },
                    { "Reason", "Invalid email or password" }
                });
            return Unauthorized(new { message = "Invalid email or password" });
        }

        logger.LogInformation("Successful authorization for user {Email}. UserName: {UserName}", request.Email, loginResponse.UserName);

        customLogger.Info("Successful authorization", LoggerNameForCustomLog,
            context: new Dictionary<string, object>
            {
                { "Email", request.Email },
                { "UserId", loginResponse.UserName }
            });

        return Ok(loginResponse);
    }
}