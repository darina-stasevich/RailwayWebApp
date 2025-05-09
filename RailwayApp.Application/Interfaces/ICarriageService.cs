using RailwayApp.Application.Models;

namespace RailwayApp.Domain.Interfaces.IServices;

public interface ICarriageService
{
    Task<IEnumerable<ShortCarriageInfoDto>> GetAllCarriagesInfo(CarriagesInfoRequest request);
    Task<DetailedCarriageInfoDto> GetCarriageInfo(CarriageInfoRequest request);
}