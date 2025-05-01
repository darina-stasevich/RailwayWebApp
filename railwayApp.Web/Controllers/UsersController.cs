using Microsoft.AspNetCore.Mvc;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces.IRepositories;
using RailwayApp.Infrastructure.Repositories;

namespace RailwayApp.Web.Controllers;

// UsersController.cs
[ApiController]
[Route("api/users")]
public class UsersController(IUserAccountRepository repository) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateUser(UserAccount user)
    {
        await repository.CreateAsync(user);
        return CreatedAtAction(nameof(GetByEmail), new { email = user.Email }, user);
    }

    [HttpGet("{email}")]
    public async Task<IActionResult> GetByEmail(string email)
    {
        var user = await repository.GetByEmailAsync(email);
        return user == null ? NotFound() : Ok(user);
    }
}