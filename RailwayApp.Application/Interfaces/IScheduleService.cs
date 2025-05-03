using RailwayApp.Application.Models;

namespace RailwayApp.Domain.Interfaces.IServices;

public interface IScheduleService
{
    Task<ScheduleDto> GetScheduleAsync(Guid concreteRouteId);
}