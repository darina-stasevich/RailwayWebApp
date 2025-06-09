using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces.IServices.AdminServices;

namespace RailwayApp.Web.Controllers.AdminControllers;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminUserAccountsController(
    IAdminUserAccountService adminUserAccountService,
    ILogger<AdminUserAccountsController> logger)
    : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserAccount>>> GetAllUserAccounts()
    {
        logger.LogInformation("Admin: User attempting to retrieve all user accounts.");
        var items = await adminUserAccountService.GetAllItems();
        logger.LogInformation("Admin: User successfully retrieved {Count} user accounts.", items.Count());
        return Ok(items);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserAccount>> GetUserAccountById(Guid id)
    {
        logger.LogInformation("Admin: User attempting to retrieve user account with ID: {Id}.", id);
        
        var userAccount = await adminUserAccountService.GetItemByIdAsync(id); 
        
        logger.LogInformation("Admin: User successfully retrieved user account with ID: {Id}.", id);
        return Ok(userAccount);
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> CreateUserAccount([FromBody] UserAccount userAccount)
    {
        if (!ModelState.IsValid)
        {
            logger.LogWarning("Admin: User - Model validation failed for creating user account: {ModelStateErrors}. Request: {Request}.",
                JsonSerializer.Serialize(ModelState), JsonSerializer.Serialize(userAccount));
            return BadRequest(ModelState);
        }

        logger.LogInformation("Admin: User attempting to create user account: {UserAccount}.", JsonSerializer.Serialize(userAccount));
        var newId = await adminUserAccountService.CreateItem(userAccount);
        logger.LogInformation("Admin: User successfully created user account with ID: {Id}.", newId);
        return CreatedAtAction(nameof(GetUserAccountById), new { id = newId }, newId);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateUserAccount(Guid id, [FromBody] UserAccount userAccount)
    {
        if (!ModelState.IsValid)
        {
            logger.LogWarning("Admin: User - Model validation failed for updating user account ID {Id}: {ModelStateErrors}. Request: {Request}.",
                id, JsonSerializer.Serialize(ModelState), JsonSerializer.Serialize(userAccount));
            return BadRequest(ModelState);
        }

        if (id != userAccount.Id)
        {
            logger.LogWarning("Admin: User - Mismatch between ID from URL ({UrlId}) and payload ID ({PayloadId}) for updating user account.",
                id, userAccount.Id);
            return BadRequest(new ProblemDetails 
            { 
                Title = "ID Mismatch", 
                Detail = "ID in URL must match ID in request body.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        logger.LogInformation("Admin: User attempting to update user account with ID: {Id}. Data: {UserAccount}.", id, JsonSerializer.Serialize(userAccount));
        await adminUserAccountService.UpdateItem(id, userAccount);
        logger.LogInformation("Admin: User successfully updated user account with ID: {Id}.", id);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteUserAccount(Guid id)
    {
        logger.LogInformation("Admin: User attempting to delete user account with ID: {Id}.", id);
        await adminUserAccountService.DeleteItem(id);
        logger.LogInformation("Admin: User successfully deleted user account with ID: {Id}.", id);
        return NoContent();
    }   
}