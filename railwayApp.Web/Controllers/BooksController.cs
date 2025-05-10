using System.Net.Security;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RailwayApp.Application.Models;
using RailwayApp.Domain;
using RailwayApp.Domain.Interfaces.IServices;
using RailwayApp.Domain.Statuses;

namespace RailwayApp.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Client")]
public class BooksController(ITicketBookingService ticketBookingService,
    ILogger<BooksController> logger) : ControllerBase
{
    [HttpPost("bookSeats")]
    public async Task<ActionResult<Guid>> BookSeats([FromBody] List<BookSeatRequest> requests)
    {
        if (!ModelState.IsValid)
        {
            var requestJson = JsonSerializer.Serialize(requests);
            logger.LogWarning("Model validation failed for route search request: {ModelStateErrors}. Request: {RouteRequest}", JsonSerializer.Serialize(ModelState), requestJson);
            return ValidationProblem(ModelState);
        }

        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out Guid userAccountId)) 
            return Unauthorized("User ID claim is missing or invalid.");
        
        try
        {
            var requestJson = JsonSerializer.Serialize(requests);
            logger.LogInformation("start searching book seats for request {requests}", requestJson);

            var result = await ticketBookingService.BookPlaces(userAccountId, requests);

            logger.LogInformation("successfully book seats for request {requests}, get seatLockId {id}", requestJson,
                result);

            return Ok(result);
        }
        catch (UserServiceUserNotFoundException ex)
        {
            logger.LogWarning("user with id {id} not found", userAccountId);
            return NotFound(ex.Message);
        }
        catch (UserServiceUserBlockedException ex)
        {
            logger.LogWarning("user with id {id} blocked", userAccountId);
            return BadRequest(ex.Message);
        }
        catch (TicketBookingServiceSeatNotAvailableException ex)
        {
            logger.LogWarning("one or more required seats is already blocked: {ex}", ex.Message);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError("Unhandled exception caught {ex}", ex.Message);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("cancelBooks")]
    public async Task<ActionResult<bool>> CancelBookSeats(Guid seatLockId)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out Guid userAccountId)) 
            return Unauthorized("User ID claim is missing or invalid.");
        
        try
        {
            logger.LogInformation("try to cancel seat lock {seatLockId}", seatLockId);
            var result = await ticketBookingService.CancelBookPlaces(userAccountId, seatLockId);
            logger.LogInformation("cancel of lock {seatLockId} successful", seatLockId);
            return result;

        }
        catch (UserServiceUserNotFoundException ex)
        {
            logger.LogWarning("user with id {id} not found", userAccountId);
            return NotFound(ex.Message);
        }
        catch (UserServiceUserBlockedException ex)
        {
            logger.LogWarning("user with id {id} blocked", userAccountId);
            return BadRequest(ex.Message);
        }
        catch (TicketBookingServiceSeatLockNotFoundException ex)
        {
            logger.LogWarning("booking was not found: {ex}", ex.Message);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError("Unhandled exception caught {ex}", ex.Message);
            return StatusCode(500, "Internal server error");
        }
        
    }
}