using Microsoft.AspNetCore.Mvc;
using RailwayApp.Application.Models;
using RailwayApp.Domain;
using RailwayApp.Domain.Interfaces.IServices;

namespace RailwayApp.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserAccountsController(IUserAccountService userAccountService,
    ILogger<UserAccountsController> logger) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> CreateUserAccount([FromBody] CreateUserAccountRequest request)
    {
        if (!ModelState.IsValid)
        {
            logger.LogWarning("Incorrect request: {@Errors}", ModelState);
            return BadRequest(ModelState);
        }

        try
        {
            var userAccountId = await userAccountService.CreateUserAccountAsync(request);
            logger.LogInformation("User account with id {Id} was created", userAccountId);
            return Ok(userAccountId);
        }
        catch (UserServiceEmailAlreadyExistsException ex)
        {
            logger.LogWarning(ex, "Error creating user account: {Message}", ex.Message);
            return Conflict(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating user account: {Message}", ex.Message);
            return StatusCode(500, "Internal Server Error");
        }
    }
    
    /*[HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetMyProfile()
    {
        var userId = GetCurrentUserId();
        _logger.LogInformation("Attempting to get profile for UserId {UserId}", userId);

        UserDto userDto = await _userService.GetUserProfileAsync(userId);

        _logger.LogInformation("Profile retrieved successfully for UserId {UserId}", userId);
        return Ok(userDto);
    }*/

    /*[HttpGet("{id}")]
    public async Task<IActionResult> GetUserAccount(Guid id)
    {
        var userAccount = await userAccountService.GetByUserAccountIdAsync(id);
        if (userAccount == null)
        {
            return NotFound();
        }
        return Ok(userAccount);
    }*/

    [HttpPut("me")]
    public async Task<IActionResult> UpdateUserAccount([FromBody] UpdateUserAccountRequest request)
    {
        if (!ModelState.IsValid)
        {
            logger.LogWarning("Incorrect request: {@Errors}", ModelState);
            return BadRequest(ModelState);
        }

        var id = Guid.Parse("0af57bc9-0f1a-4836-8fcb-697e14df1c9f");
        
        try
        {
            var userAccountId = await userAccountService.UpdateUserAccountAsync(id, request);
            logger.LogInformation("data of user account with id {Id} was updated", id);
            return Ok(userAccountId);
        }
        catch (UserServiceUserNotFoundException ex)
        {
            logger.LogWarning(ex, "Error updating user account: {Message}", ex.Message);
            return NotFound(ex.Message);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    [HttpPut("me/password")]
    public async Task<IActionResult> UpdateUserAccount([FromBody] ChangePasswordRequest request)
    {
        if (!ModelState.IsValid)
        {
            logger.LogWarning("Incorrect request: {@Errors}", ModelState);
            return BadRequest(ModelState);
        }
        
        var id = Guid.Parse("0af57bc9-0f1a-4836-8fcb-697e14df1c9f");
        
        try
        {
            var userAccountId = await userAccountService.UpdateUserPasswordAsync(id, request);
            logger.LogInformation("data of user account with id {Id} was updated", id);
            return Ok(userAccountId);
        }
        catch (UserServiceUserNotFoundException ex)
        {
            logger.LogWarning(ex, "Error updating user account: {Message}", ex.Message);
            return NotFound(ex.Message);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    
    [HttpDelete("me")]
    public async Task<IActionResult> DeleteUserAccount(Guid id)
    {
        try
        {
            await userAccountService.DeleteUserAccountAsync(id);
            logger.LogInformation("User account with id {Id} was deleted", id);
            return NoContent();
        }
        catch(UserServiceUserNotFoundException ex)
        {
            logger.LogWarning(ex, "Error deleting user account: {Message}", ex.Message);
            return NotFound(ex.Message);
        }
        catch (UserServiceUserBlockedException ex)
        {
            logger.LogWarning(ex, "Error deleting user account: {Message}", ex.Message);
            return Conflict(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting user account: {Message}", ex.Message);
            return StatusCode(500, "Internal Server Error");
        }
    }
}