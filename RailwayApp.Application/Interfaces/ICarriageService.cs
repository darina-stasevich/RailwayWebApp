using RailwayApp.Application.Models;

namespace RailwayApp.Domain.Interfaces.IServices;

public interface ICarriageService
{
    Task<List<ShortCarriageInfoDto>> GetAllCarriagesInfo(Guid concreteRouteId, int startSegmentNumber, int endSegmentNumber);

    Task<DetailedCarriageInfoDto> GetCarriageInfo(Guid concreteRouteId, int startSegmentNumber, int endSegmentNumber,
        int carriageNumber);
}