using System.Diagnostics;
using MongoDB.Driver;
using RailwayApp.Application.Models.Dto;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces.IRepositories;
using RailwayApp.Domain.Interfaces.IServices;
using RailwayApp.Infrastructure.Repositories;

namespace RailwayApp.Application.Services;

public class CarriageAvailabilityUpdateService(ICarriageService carriageService,
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
            Console.WriteLine("ok ok ok ook ook ");
            Debug.WriteLine("ok ok ok ok ok");
            Console.WriteLine(carriageAvailabilities.Count());
            Debug.WriteLine(carriageAvailabilities.Count());
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
}