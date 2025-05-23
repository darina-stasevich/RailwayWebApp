using System.Security.Claims;
// using CustomSerializer; // Этот using есть, но не используется в предоставленном коде контроллера. Оставляю на всякий случай.
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using railway_service;
using RailwayApp.Application.Models; // Для CreateUserAccountRequest, UpdateUserAccountRequest, ChangePasswordRequest
using RailwayApp.Domain.Interfaces.IServices; // Для IUserAccountService
// Предполагается, что IMyCustomLogger и его реализация (MyCustomFileJsonLogger) определены
// и зарегистрированы в DI.
// Например, в Program.cs:
// builder.Services.AddSingleton<IMyCustomLogger, MyCustomFileJsonLogger>();

namespace RailwayApp.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserAccountsController(
    IUserAccountService userAccountService,
    ILogger<UserAccountsController> logger,
    IMyCustomLogger customLogger)
    : ControllerBase
{
    private const string LoggerNameForCustomLog = "UserAccountsController"; // Имя для кастомного логгера
    
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> CreateUserAccount([FromBody] CreateUserAccountRequest request)
    {
        if (!ModelState.IsValid)
        {
            // Стандартное логирование (Serilog)
            logger.LogWarning("Incorrect request for user account creation: {@Errors}. Request Data: {@Request}", ModelState, request);

            // Твое кастомное логирование
            var modelStateErrors = ModelState.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
            );
            customLogger.Warn("Incorrect request for user account creation", LoggerNameForCustomLog,
                exception: null,
                context: new Dictionary<string, object>
                {
                    { "ModelStateErrors", modelStateErrors },
                    { "AttemptedEmail", request?.Email }, // Email важен при регистрации
                    { "FailedRequestData", request }
                });
            return BadRequest(ModelState);
        }

        // Стандартное логирование (Serilog) - перед вызовом
        logger.LogInformation("Attempting to create user account for Email: {Email}", request.Email);
        // Твое кастомное логирование - перед вызовом
        customLogger.Info("Attempting to create user account", LoggerNameForCustomLog,
            context: new Dictionary<string, object> { { "Email", request.Email }, { "RequestData", request } });

        var userAccountId = await userAccountService.CreateUserAccountAsync(request);

        // Стандартное логирование (Serilog) - после вызова
        logger.LogInformation("User account with Id {Id} was created for Email: {Email}", userAccountId, request.Email);
        // Твое кастомное логирование - после вызова
        customLogger.Info("User account created successfully", LoggerNameForCustomLog,
            context: new Dictionary<string, object>
            {
                { "UserAccountId", userAccountId },
                { "Email", request.Email }
            });
        return Ok(userAccountId);
    }

    [HttpGet("me")]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> GetUserAccount()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userAccountId))
        {
            // Стандартное логирование (Serilog)
            logger.LogWarning("User ID claim is missing or invalid in GetUserAccount request. Path: {Path}", HttpContext.Request.Path);
            // Твое кастомное логирование
            customLogger.Warn("User ID claim is missing or invalid in GetUserAccount request", LoggerNameForCustomLog,
                exception: null,
                context: new Dictionary<string, object> { { "Path", HttpContext.Request.Path.ToString() } });
            return Unauthorized("User ID claim is missing or invalid.");
        }

        // Стандартное логирование (Serilog) - перед вызовом
        logger.LogInformation("Attempting to retrieve user account for Id: {UserAccountId}", userAccountId);
        // Твое кастомное логирование - перед вызовом
        customLogger.Info("Attempting to retrieve user account", LoggerNameForCustomLog,
            context: new Dictionary<string, object> { { "UserAccountId", userAccountId } });

        var userAccountDto = await userAccountService.GetUserAccount(userAccountId);

        if (userAccountDto == null)
        {
            logger.LogWarning("User account not found for Id: {UserAccountId}", userAccountId);
            customLogger.Warn("User account not found", LoggerNameForCustomLog, exception: null,
                context: new Dictionary<string, object> { { "UserAccountId", userAccountId } });
            return NotFound("User account not found.");
        }
        // Стандартное логирование (Serilog) - после вызова
        logger.LogInformation("User account with Id {Id} was retrieved", userAccountId);
        // Твое кастомное логирование - после вызова
        customLogger.Info("User account retrieved successfully", LoggerNameForCustomLog,
            context: new Dictionary<string, object>
            {
                { "UserAccountId", userAccountId },
                // { "RetrievedAccountData", userAccountDto } // Можно добавить, если не слишком много данных
            });
        return Ok(userAccountDto);
    }

    [HttpPut("me")]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> UpdateUserAccount([FromBody] UpdateUserAccountRequest request)
    {
        if (!ModelState.IsValid)
        {
            // Стандартное логирование (Serilog)
            logger.LogWarning("Incorrect request for user account update: {@Errors}. Request Data: {@Request}", ModelState, request);
            // Твое кастомное логирование
            var modelStateErrors = ModelState.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
            );
            customLogger.Warn("Incorrect request for user account update", LoggerNameForCustomLog,
                exception: null,
                context: new Dictionary<string, object>
                {
                    { "ModelStateErrors", modelStateErrors },
                    { "FailedRequestData", request }
                });
            return BadRequest(ModelState);
        }

        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userAccountId))
        {
            // Стандартное логирование (Serilog)
            logger.LogWarning("User ID claim is missing or invalid in UpdateUserAccount request. Path: {Path}", HttpContext.Request.Path);
            // Твое кастомное логирование
            customLogger.Warn("User ID claim is missing or invalid in UpdateUserAccount request", LoggerNameForCustomLog,
                exception: null,
                context: new Dictionary<string, object> { { "Path", HttpContext.Request.Path.ToString() } });
            return Unauthorized("User ID claim is missing or invalid.");
        }

        // Стандартное логирование (Serilog) - перед вызовом
        logger.LogInformation("Attempting to update user account for Id: {UserAccountId}. Request: {@Request}", userAccountId, request);
        // Твое кастомное логирование - перед вызовом
        customLogger.Info("Attempting to update user account", LoggerNameForCustomLog,
            context: new Dictionary<string, object> { { "UserAccountId", userAccountId }, { "UpdateRequest", request } });

        var userAccount = await userAccountService.UpdateUserAccountAsync(userAccountId, request);

        // Стандартное логирование (Serilog) - после вызова
        logger.LogInformation("Data of user account with Id {Id} was updated", userAccountId);
        // Твое кастомное логирование - после вызова
        customLogger.Info("User account data updated successfully", LoggerNameForCustomLog,
            context: new Dictionary<string, object>
            {
                { "UserAccountId", userAccountId },
                // { "UpdatedAccountData", userAccount } // Можно добавить, если не слишком много данных
            });
        return Ok(userAccount);
    }

    [HttpPut("me/password")]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> UpdateUserPassword([FromBody] ChangePasswordRequest request) // Изменил имя метода для ясности
    {
        if (!ModelState.IsValid)
        {
            // Стандартное логирование (Serilog)
            logger.LogWarning("Incorrect request for user password change: {@Errors}", ModelState); // Не логируем сам request с паролями
            // Твое кастомное логирование
            var modelStateErrors = ModelState.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
            );
            customLogger.Warn("Incorrect request for user password change", LoggerNameForCustomLog,
                exception: null,
                context: new Dictionary<string, object> { { "ModelStateErrors", modelStateErrors } }); // Не логируем сам request с паролями
            return BadRequest(ModelState);
        }

        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userAccountId))
        {
            // Стандартное логирование (Serilog)
            logger.LogWarning("User ID claim is missing or invalid in UpdateUserPassword request. Path: {Path}", HttpContext.Request.Path);
            // Твое кастомное логирование
            customLogger.Warn("User ID claim is missing or invalid in UpdateUserPassword request", LoggerNameForCustomLog,
                exception: null,
                context: new Dictionary<string, object> { { "Path", HttpContext.Request.Path.ToString() } });
            return Unauthorized("User ID claim is missing or invalid.");
        }

        // Стандартное логирование (Serilog) - перед вызовом
        logger.LogInformation("Attempting to update password for user Id: {UserAccountId}", userAccountId);
        // Твое кастомное логирование - перед вызовом
        customLogger.Info("Attempting to update user password", LoggerNameForCustomLog,
            context: new Dictionary<string, object> { { "UserAccountId", userAccountId } }); // Не логируем объект request с паролями

        var updatedUserAccount = await userAccountService.UpdateUserPasswordAsync(userAccountId, request); // Сервис должен вернуть обновленный объект или bool

        // Стандартное логирование (Serilog) - после вызова
        logger.LogInformation("Password for user account with Id {Id} was updated", userAccountId);
        // Твое кастомное логирование - после вызова
        customLogger.Info("User account password updated successfully", LoggerNameForCustomLog,
            context: new Dictionary<string, object> { { "UserAccountId", userAccountId } });
        // В зависимости от того, что возвращает UpdateUserPasswordAsync, можно вернуть Ok(updatedUserAccount) или просто Ok()
        return Ok(updatedUserAccount); // или Ok("Password updated successfully");
    }

    [HttpDelete("me")]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> DeleteUserAccount()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userAccountId))
        {
            // Стандартное логирование (Serilog)
            logger.LogWarning("User ID claim is missing or invalid in DeleteUserAccount request. Path: {Path}", HttpContext.Request.Path);
            // Твое кастомное логирование
            customLogger.Warn("User ID claim is missing or invalid in DeleteUserAccount request", LoggerNameForCustomLog,
                exception: null,
                context: new Dictionary<string, object> { { "Path", HttpContext.Request.Path.ToString() } });
            return Unauthorized("User ID claim is missing or invalid.");
        }

        // Стандартное логирование (Serilog) - перед вызовом
        logger.LogInformation("Attempting to delete user account for Id: {UserAccountId}", userAccountId);
        // Твое кастомное логирование - перед вызовом
        customLogger.Info("Attempting to delete user account", LoggerNameForCustomLog,
            context: new Dictionary<string, object> { { "UserAccountId", userAccountId } });

        await userAccountService.DeleteUserAccountAsync(userAccountId);

        // Стандартное логирование (Serilog) - после вызова
        logger.LogInformation("User account with Id {Id} was deleted", userAccountId);
        // Твое кастомное логирование - после вызова
        customLogger.Info("User account deleted successfully", LoggerNameForCustomLog,
            context: new Dictionary<string, object> { { "UserAccountId", userAccountId } });
        return NoContent();
    }
}