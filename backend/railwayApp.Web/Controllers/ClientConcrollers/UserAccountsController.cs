using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RailwayApp.Application.Models;
using RailwayApp.Domain.Interfaces.IServices;

namespace RailwayApp.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserAccountsController(
    IUserAccountService userAccountService,
    ILogger<UserAccountsController> logger) : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> CreateUserAccount([FromBody] CreateUserAccountRequest request)
    {
        if (!ModelState.IsValid)
        {
            logger.LogWarning("Incorrect request: {@Errors}", ModelState);
            return BadRequest(ModelState);
        }

        var userAccountId = await userAccountService.CreateUserAccountAsync(request);
        logger.LogInformation("User account with id {Id} was created", userAccountId);
        return Ok(userAccountId);
    }

    [HttpGet("me")]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> GetUserAccount()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var id))
            return Unauthorized("User ID claim is missing or invalid.");
        var userAccountDto = await userAccountService.GetUserAccount(id);
        logger.LogInformation("User account with id {Id} was retrieved", id);
        return Ok(userAccountDto);
    }
    
    [HttpPut("me")]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> UpdateUserAccount([FromBody] UpdateUserAccountRequest request)
    {
        if (!ModelState.IsValid)
        {
            logger.LogWarning("Incorrect request: {@Errors}", ModelState);
            return BadRequest(ModelState);
        }

        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var id))
            return Unauthorized("User ID claim is missing or invalid.");
        var userAccount = await userAccountService.UpdateUserAccountAsync(id, request);
        logger.LogInformation("data of user account with id {Id} was updated", id);
        return Ok(userAccount);
    }

    [HttpPut("me/password")]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> UpdateUserAccount([FromBody] ChangePasswordRequest request)
    {
        if (!ModelState.IsValid)
        {
            logger.LogWarning("Incorrect request: {@Errors}", ModelState);
            return BadRequest(ModelState);
        }

        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var id))
            return Unauthorized("User ID claim is missing or invalid.");

        var userAccountId = await userAccountService.UpdateUserPasswordAsync(id, request);
        logger.LogInformation("data of user account with id {Id} was updated", id);
        return Ok(userAccountId);
    }

    [HttpDelete("me")]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> DeleteUserAccount()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var id))
            return Unauthorized("User ID claim is missing or invalid.");

        await userAccountService.DeleteUserAccountAsync(id);
        logger.LogInformation("User account with id {Id} was deleted", id);
        return NoContent();
    }
}