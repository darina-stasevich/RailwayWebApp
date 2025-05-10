using MongoDB.Driver;
using RailwayApp.Application.Models.Dto;
using RailwayApp.Domain.Entities;

namespace RailwayApp.Domain.Interfaces.IServices;

public interface ICarriageAvailabilityUpdateService
{
    Task<bool> MarkSeatsAsOccupied(List<LockedSeatInfo> lockedSeatInfos, IClientSessionHandle session);
    Task<bool> MarkSeatAsFree(FreeTicketDto dto, IClientSessionHandle session);
}