using MongoDB.Driver;
using RailwayApp.Domain.Entities;

namespace RailwayApp.Domain.Interfaces.IServices;

public interface ICarriageTemplateService
{
    public Task<IEnumerable<CarriageTemplate>?> GetCarriageTemplateForRouteAsync(Guid concreteRouteId, IClientSessionHandle? session = null);

}