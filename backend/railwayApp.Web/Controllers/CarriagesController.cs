using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RailwayApp.Application.Models;
using RailwayApp.Domain.Interfaces.IServices;

namespace RailwayApp.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Client")]
public class CarriagesController(
    ICarriageService carriageService,
    ILogger<CarriagesController> logger) : ControllerBase
{
    [HttpPost("summaries")]
    public async Task<ActionResult<List<ShortCarriageInfoDto>>> GetCarriageSummaries(
        [FromBody] CarriagesInfoRequest request)
    {
        if (!ModelState.IsValid)
        {
            var requestJson = JsonSerializer.Serialize(request);
            logger.LogWarning(
                "Model validation failed for route search request: {ModelStateErrors}. Request: {RouteRequest}",
                JsonSerializer.Serialize(ModelState), requestJson);
            return ValidationProblem(ModelState);
        }

        logger.LogInformation(
            "Getting carriages info for ConcreteRouteId: {ConcreteRouteId}, StartSegmentNumber: {StartSegmentNumber}, EndSegmentNumber: {EndSegmentNumber}",
            request.ConcreteRouteId, request.StartSegmentNumber, request.EndSegmentNumber);

        var carriages = await carriageService.GetAllCarriagesInfo(request);

        logger.LogInformation("Successfully retrieved {CarriageCount} carriages for request: {CarriageRequest}",
            carriages.Count(), JsonSerializer.Serialize(request));
        return Ok(carriages);
    }

    [HttpPost("details")]
    public async Task<ActionResult<DetailedCarriageInfoDto>> GetCarriageDetails([FromBody] CarriageInfoRequest request)
    {
        if (!ModelState.IsValid)
        {
            var requestJson = JsonSerializer.Serialize(request);
            logger.LogWarning(
                "Model validation failed for route search request: {ModelStateErrors}. Request: {RouteRequest}",
                JsonSerializer.Serialize(ModelState), requestJson);
            return ValidationProblem(ModelState);
        }

        logger.LogInformation(
            "Getting carriages info for ConcreteRouteId: {ConcreteRouteId}, StartSegmentNumber: {StartSegmentNumber}, EndSegmentNumber: {EndSegmentNumber}, CarriageNumber: {CarriageNumber}",
            request.ConcreteRouteId, request.StartSegmentNumber, request.EndSegmentNumber, request.CarriageNumber);

        var carriage = await carriageService.GetCarriageInfo(request);

        logger.LogInformation("Successfully retrieved carriages for request: {CarriageRequest}",
            JsonSerializer.Serialize(request));
        return Ok(carriage);
    }
}