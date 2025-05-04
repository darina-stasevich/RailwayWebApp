using RailwayApp.Domain.Entities;

namespace RailwayApp.Domain.Interfaces.IServices;

public interface ICarriageTemplateService
{
    public Task<List<CarriageTemplate>> GetCarriageTemplateForRouteAsync(Guid concreteRouteId);

}