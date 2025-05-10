using System.Diagnostics;
using MongoDB.Driver;
using RailwayApp.Application.Models.Dto;
using RailwayApp.Domain;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces.IRepositories;
using RailwayApp.Domain.Interfaces.IServices;
using RailwayApp.Infrastructure.Repositories;

namespace RailwayApp.Application.Services;

public class CarriageAvailabilityUpdateService(ICarriageService carriageService,
    ICarriageTemplateService carriageTemplateService,
    ICarriageAvailabilityRepository carriageAvailabilityRepository) : ICarriageAvailabilityUpdateService
{
    public async Task<bool> MarkSeatsAsOccupied(List<LockedSeatInfo> lockedSeatInfos, IClientSessionHandle session)
    {
        foreach (var seatInfo in lockedSeatInfos)
        {
            var dto = new OccupiedSeatDto
            {
                CarriageTemplateId = seatInfo.CarriageTemplateId,
                StartSegmentNumber = seatInfo.StartSegmentNumber,
                EndSegmentNumber = seatInfo.EndSegmentNumber,
                ConcreteRouteId = seatInfo.ConcreteRouteId
            };
            var carriageAvailabilities = await carriageService.GetCarriageAvailabilitiesForSeat(dto, session);

            foreach (var carriageAvailability in carriageAvailabilities)
            {
                if (carriageAvailability.OccupiedSeats[seatInfo.SeatNumber - 1] == false)
                    throw new Domain.CarriageAvailabilityUpdateService(
                        $"seat {seatInfo.SeatNumber} is already booked in segment {carriageAvailability.ConcreteRouteSegmentId}");
                carriageAvailability.OccupiedSeats[seatInfo.SeatNumber - 1] = false;
            }

            var updateResult =
                await carriageAvailabilityRepository.UpdateOccupiedSeats(carriageAvailabilities, session);
            if (!updateResult)
                return false;
        }

        return true;
    }

    public async Task<bool> MarkSeatAsFree(FreeTicketDto dto, IClientSessionHandle session)
    {
        var carriageTemplates =
            await carriageTemplateService.GetCarriageTemplateForRouteAsync(dto.ConcreteRouteId, session);
        var carriageTemplate = carriageTemplates.FirstOrDefault(x => x.CarriageNumber == dto.Carriage);
        if(carriageTemplate == null)
            throw new CarriageTemplateNotFoundException($"Carriage template for route {dto.ConcreteRouteId} with carriage number {dto.Carriage} not found");
        var seatDto = new OccupiedSeatDto
        {
            CarriageTemplateId = carriageTemplate.Id,
            StartSegmentNumber = dto.StartSegmentNumber,
            EndSegmentNumber = dto.EndSegmentNumber,
            ConcreteRouteId = dto.ConcreteRouteId
        };
        var carriageAvailabilities = await carriageService.GetCarriageAvailabilitiesForSeat(seatDto, session);
        foreach (var carriageAvailability in carriageAvailabilities)
        {
            if (carriageAvailability.OccupiedSeats[dto.SeatNumber - 1] == true)
                throw new Domain.CarriageAvailabilityUpdateService(
                    $"seat {dto.SeatNumber} is already booked in segment {carriageAvailability.ConcreteRouteSegmentId}");
            carriageAvailability.OccupiedSeats[dto.SeatNumber - 1] = true;
        }

        var updateResult =
            await carriageAvailabilityRepository.UpdateOccupiedSeats(carriageAvailabilities, session);
        return updateResult;
    }
}