using RailwayApp.Application.Models;

namespace RailwayApp.Domain.Interfaces.IServices;

public interface IScheduleService
{
    Task<ScheduleDto> GetScheduleAsync(Guid concreteRouteId);
    Task<DateTime> GetDepartureDateForSegment(Guid concreteRouteId, int segmentNumber);
    Task<DateTime> GetArrivalDateForSegment(Guid concreteRouteId, int segmentNumber);
}