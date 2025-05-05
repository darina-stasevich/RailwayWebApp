using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using RailwayApp.Application.Models;
using RailwayApp.Domain.Interfaces.IServices;

namespace RailwayApp.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CarriagesController(ICarriageService carriageService,
    ILogger<CarriagesController> logger) : ControllerBase
{
    [HttpPost("summaries")]
    public async Task<ActionResult<List<ShortCarriageInfoDto>>> GetCarriageSummaries([FromBody] CarriagesInfoRequest request)
    {
        if (!ModelState.IsValid)
        {
            var requestJson = JsonSerializer.Serialize(request);
            logger.LogWarning("Model validation failed for route search request: {ModelStateErrors}. Request: {RouteRequest}", JsonSerializer.Serialize(ModelState), requestJson);
            return ValidationProblem(ModelState);
        }   
        
        try
        {
            logger.LogInformation("Getting carriages info for ConcreteRouteId: {ConcreteRouteId}, StartSegmentNumber: {StartSegmentNumber}, EndSegmentNumber: {EndSegmentNumber}",
                request.ConcreteRouteId, request.StartSegmentNumber, request.EndSegmentNumber);
            
            var carriages = await carriageService.GetAllCarriagesInfo(request);
            
            logger.LogInformation("Successfully retrieved {CarriageCount} carriages for request: {CarriageRequest}", carriages.Count, JsonSerializer.Serialize(request));
            return Ok(carriages);
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning(ex, "ArgumentException caught during route search for request: {CarriageRequest}", JsonSerializer.Serialize(request));
            return BadRequest();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception occured {message}", ex.Message);
            return StatusCode(StatusCodes.Status404NotFound, "Internal Server Error");
        }
        
    }
    
    [HttpPost("details")]
    public async Task<ActionResult<DetailedCarriageInfoDto>> GetCarriageDetails([FromBody] CarriageInfoRequest request)
    {
        if (!ModelState.IsValid)
        {
            var requestJson = JsonSerializer.Serialize(request);
            logger.LogWarning("Model validation failed for route search request: {ModelStateErrors}. Request: {RouteRequest}", JsonSerializer.Serialize(ModelState), requestJson);
            return ValidationProblem(ModelState);
        }

        try
        {
            logger.LogInformation(
                "Getting carriages info for ConcreteRouteId: {ConcreteRouteId}, StartSegmentNumber: {StartSegmentNumber}, EndSegmentNumber: {EndSegmentNumber}, CarriageNumber: {CarriageNumber}",
                request.ConcreteRouteId, request.StartSegmentNumber, request.EndSegmentNumber, request.CarriageNumber);

            var carriage = await carriageService.GetCarriageInfo(request);

            logger.LogInformation("Successfully retrieved carriages for request: {CarriageRequest}",
                JsonSerializer.Serialize(request));
            return Ok(carriage);
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning(ex, "ArgumentException caught during route search for request: {CarriageRequest}",
                JsonSerializer.Serialize(request));
            return BadRequest();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception occured {message}", ex.Message);
            return StatusCode(500, "Internal Server Error");
        }
    }
}