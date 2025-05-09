using System.Net.Security;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using RailwayApp.Application.Models;
using RailwayApp.Domain;
using RailwayApp.Domain.Interfaces.IServices;

namespace RailwayApp.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
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

        var userAccountId = Guid.Parse("dd656aed-f0f0-4c09-b97b-79ba6a01c1c7"); // replace with normal claim

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
    public async Task<ActionResult<bool>> CancelBookSeats([FromBody] Guid seatlockId)
    {
        var userAccountId = Guid.Parse("dd656aed-f0f0-4c09-b97b-79ba6a01c1c7"); // replace with normal claim

        try
        {
            logger.LogInformation("try to cancel seat lock {seatLockId}", seatlockId);
            var result = await ticketBookingService.CancelBookPlaces(userAccountId, seatlockId);
            logger.LogInformation("cancel of lock {seatLockId} successful", seatlockId);
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