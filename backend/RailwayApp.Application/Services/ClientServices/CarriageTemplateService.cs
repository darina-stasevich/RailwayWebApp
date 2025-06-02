using MongoDB.Driver;
using RailwayApp.Domain;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces.IRepositories;
using RailwayApp.Domain.Interfaces.IServices;

namespace RailwayApp.Application.Services;

public class CarriageTemplateService(
    IUnitOfWork unitOfWork) : ICarriageTemplateService
{
    public async Task<IEnumerable<CarriageTemplate>?> GetCarriageTemplateForRouteAsync(Guid concreteRouteId,
        IClientSessionHandle? session = null)
    {
        var concreteRoute = await unitOfWork.ConcreteRoutes.GetByIdAsync(concreteRouteId, session);
        if (concreteRoute == null) throw new ConcreteRouteNotFoundException(concreteRouteId);

        var abstractRoute = await unitOfWork.AbstractRoutes.GetByIdAsync(concreteRoute.AbstractRouteId, session);
        if (abstractRoute == null) throw new AbstractRouteNotFoundException(concreteRoute.AbstractRouteId);

        var train = await unitOfWork.Trains.GetByIdAsync(abstractRoute.TrainNumber, session);
        if (train == null) throw new TrainNotFoundException(abstractRoute.TrainNumber);

        var carriageTemplates =
            await unitOfWork.CarriageTemplates.GetByTrainTypeIdAsync(train.TrainTypeId, session);

        return carriageTemplates;
    }
}