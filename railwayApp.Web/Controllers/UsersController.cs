using Microsoft.AspNetCore.Mvc;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces.IRepositories;
using RailwayApp.Infrastructure.Repositories;

namespace RailwayApp.Web.Controllers;

// UsersController.cs
[ApiController]
[Route("api/users")]
public class UsersController(IUserAccountRepository repository,
    ILogger<UsersController> logger) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateUser(UserAccount user)
    {
        try
        {
            await repository.CreateAsync(user);
            var result = CreatedAtAction(nameof(GetByEmail), new { email = user.Email }, user);
            logger.LogInformation($"User {user.Email} has been created.");
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating user {user.Email}", user.Email);
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{email}")]
    public async Task<IActionResult> GetByEmail(string email)
    {
        try
        {
            var user = await repository.GetByEmailAsync(email);
            if (user == null)
            {
                logger.LogInformation($"User {email} does not exist.");
                return NotFound();
            }
            else
            {
                logger.LogInformation($"User {email} has been retrieved.", user.Email);
                return Ok(user);
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error retrieving user {email}", email); 
            return StatusCode(500, e.Message);
        }

        
    }
}