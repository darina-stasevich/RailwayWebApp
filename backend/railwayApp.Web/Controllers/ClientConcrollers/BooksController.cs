using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RailwayApp.Application.Models;
using RailwayApp.Domain.Interfaces.IServices;

namespace RailwayApp.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Client")]
public class BooksController(
    ITicketBookingService ticketBookingService,
    ILogger<BooksController> logger
) : ControllerBase
{
    [HttpPost("book-seats")]
    public async Task<ActionResult<Guid>> BookSeats([FromBody] List<BookSeatRequest> requests)
    {
        string? requestJson;
        if (!ModelState.IsValid)
        {
            requestJson = JsonSerializer.Serialize(requests);
            logger.LogWarning(
                "Model validation failed for route search request: {ModelStateErrors}. Request: {RouteRequest}",
                JsonSerializer.Serialize(ModelState), requestJson);
            return ValidationProblem(ModelState);
        }

        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userAccountId))
            return Unauthorized("User ID claim is missing or invalid.");

        requestJson = JsonSerializer.Serialize(requests);
        logger.LogInformation("start searching book seats for request {requests}", requestJson);

        var result = await ticketBookingService.BookPlaces(userAccountId, requests);

        logger.LogInformation("successfully book seats for request {requests}, get seatLockId {id}", requestJson,
            result);

        return Ok(result);
    }

    [HttpPost("cancelBooks")]
    public async Task<ActionResult<bool>> CancelBookSeats(Guid seatLockId)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userAccountId))
            return Unauthorized("User ID claim is missing or invalid.");

        logger.LogInformation("try to cancel seat lock {seatLockId}", seatLockId);
        var result = await ticketBookingService.CancelBookPlaces(userAccountId, seatLockId);
        logger.LogInformation("cancel of lock {seatLockId} successful", seatLockId);
        return result;
    }

    [HttpGet("my-bookings")]
    public async Task<ActionResult<List<SeatLockResponse>>> GetSeatLocks()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userAccountId))
            return Unauthorized("User ID claim is missing or invalid");
        logger.LogInformation("Try to get seatlocks for user {userAccountID}", userAccountId);
        var result = await ticketBookingService.GetBooks(userAccountId);
        logger.LogInformation("getting seatlocks for {userAccountId} succeded", userAccountId);
        return result.ToList();
    }
}