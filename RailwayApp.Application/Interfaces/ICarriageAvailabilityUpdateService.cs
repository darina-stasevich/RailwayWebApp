using RailwayApp.Application.Models.Dto;

namespace RailwayApp.Domain.Interfaces.IServices;

public interface ICarriageAvailabilityUpdateService
{
    Task<bool> UpdateSeatOnRoute(OccupiedSeatDto dto);
}