using MongoDB.Driver;
using RailwayApp.Domain;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces.IRepositories;
using RailwayApp.Domain.Interfaces.IServices;

namespace RailwayApp.Application.Services;

public class CarriageTemplateService(
    IConcreteRouteRepository concreteRouteRepository,
    IAbstractRouteRepository abstractRouteRepository,
    ITrainRepository trainRepository,
    ICarriageTemplateRepository carriageTemplateRepository) : ICarriageTemplateService
{
    public async Task<IEnumerable<CarriageTemplate>?> GetCarriageTemplateForRouteAsync(Guid concreteRouteId,
        IClientSessionHandle? session = null)
    {
        var concreteRoute = await concreteRouteRepository.GetByIdAsync(concreteRouteId, session);
        if (concreteRoute == null) throw new ConcreteRouteNotFoundException(concreteRouteId);

        var abstractRoute = await abstractRouteRepository.GetByIdAsync(concreteRoute.AbstractRouteId, session);
        if (abstractRoute == null) throw new AbstractRouteNotFoundException(concreteRoute.AbstractRouteId);

        var train = await trainRepository.GetByIdAsync(abstractRoute.TrainNumber, session);
        if (train == null) throw new TrainNotFoundException(abstractRoute.TrainNumber);

        var carriageTemplates =
            await carriageTemplateRepository.GetByTrainTypeIdAsync(train.TrainTypeId, session);

        return carriageTemplates;
    }
}