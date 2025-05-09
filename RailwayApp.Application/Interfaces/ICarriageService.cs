using MongoDB.Driver;
using RailwayApp.Application.Models;
using RailwayApp.Application.Models.Dto;
using RailwayApp.Domain.Entities;

namespace RailwayApp.Domain.Interfaces.IServices;

public interface ICarriageService
{
    Task<IEnumerable<ShortCarriageInfoDto>> GetAllCarriagesInfo(CarriagesInfoRequest request);
    Task<DetailedCarriageInfoDto> GetCarriageInfo(CarriageInfoRequest request);
    Task<IEnumerable<CarriageAvailability>> GetCarriageAvailabilitiesForSeat(OccupiedSeatDto dto, IClientSessionHandle session);
}