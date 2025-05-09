using MongoDB.Driver;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces.IRepositories;
using RailwayApp.Domain.Interfaces.IServices;

namespace RailwayApp.Application.Services;

public class CarriageTemplateService(IConcreteRouteRepository concreteRouteRepository,
    IAbstractRouteRepository abstractRouteRepository,
    ITrainRepository trainRepository,
    ICarriageTemplateRepository carriageTemplateRepository) : ICarriageTemplateService
{
    public async Task<IEnumerable<CarriageTemplate>> GetCarriageTemplateForRouteAsync(Guid concreteRouteId, IClientSessionHandle? session = null)
    {
        var concreteRoute = await concreteRouteRepository.GetByIdAsync(concreteRouteId, session);
        if (concreteRoute == null)
        {
            throw new Exception("Concrete route not found");
        }
        
        var abstractRoute = await abstractRouteRepository.GetByIdAsync(concreteRoute.AbstractRouteId, session);
        if (abstractRoute == null)
        {
            throw new Exception("Abstract route not found");
        }
        
        var train = await trainRepository.GetByIdAsync(abstractRoute.TrainNumber, session);
        if (train == null)
        {
            throw new Exception("Train not found");
        }
        
        var carriageTemplates =
            await carriageTemplateRepository.GetByTrainTypeIdAsync(train.TrainTypeId, session);
        if (carriageTemplates == null || !carriageTemplates.Any())
        {
            throw new Exception("Carriage templates not found");
        }

        return carriageTemplates;
    }   
}