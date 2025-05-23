using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using railway_service;
using RailwayApp.Application.Models; // Для LoginRequest
using IAuthorizationService = RailwayApp.Domain.Interfaces.IServices.IAuthorizationService; // Используем alias для ясности
// Предполагается, что IMyCustomLogger и его реализация (MyCustomFileJsonLogger) определены
// и зарегистрированы в DI.
// Например, в Program.cs:
// builder.Services.AddSingleton<IMyCustomLogger, MyCustomFileJsonLogger>();

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
            // Стандартное логирование (Serilog) не было, но можно добавить для единообразия
            logger.LogWarning("Model validation failed for login request. Email: {Email}, Errors: {ModelStateErrors}",
                request?.Email, // Может быть null, если сам request null, но ModelState не будет IsValid
                System.Text.Json.JsonSerializer.Serialize(ModelState));

            // Твое кастомное логирование
            var modelStateErrors = ModelState.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
            );
            customLogger.Warn("Model validation failed for login request", LoggerNameForCustomLog,
                exception: null,
                context: new Dictionary<string, object>
                {
                    { "ModelStateErrors", modelStateErrors },
                    { "AttemptedEmail", request?.Email }, // Добавляем email, если доступен
                    { "FailedRequestData", request }
                });
            return BadRequest(ModelState);
        }

        var loginResponse = await authorizationService.AuthorizeAsync(request);

        if (loginResponse == null)
        {
            // Стандартное логирование (Serilog)
            logger.LogWarning("Authorization failed for user {Email}. Invalid email or password.", request.Email);

            // Твое кастомное логирование
            customLogger.Warn("Authorization failed", LoggerNameForCustomLog,
                exception: null,
                context: new Dictionary<string, object>
                {
                    { "Email", request.Email },
                    { "Reason", "Invalid email or password" }
                });
            return Unauthorized(new { message = "Invalid email or password" });
        }

        // Стандартное логирование (Serilog)
        logger.LogInformation("Successful authorization for user {Email}. UserName: {UserName}", request.Email, loginResponse.UserName);

        // Твое кастомное логирование
        customLogger.Info("Successful authorization", LoggerNameForCustomLog,
            context: new Dictionary<string, object>
            {
                { "Email", request.Email },
                { "UserId", loginResponse.UserName }
                // Можно добавить другие данные из loginResponse, если они не чувствительны,
                // например, роли, но токен точно не стоит логировать.
                // { "Roles", loginResponse.Roles } // Пример
            });

        return Ok(loginResponse);
    }
}