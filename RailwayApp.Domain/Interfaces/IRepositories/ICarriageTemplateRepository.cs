using RailwayApp.Domain.Entities;

namespace RailwayApp.Domain.Interfaces.IRepositories;

public interface ICarriageTemplateRepository
{
    Task DeleteAllAsync();
    Task<Guid> CreateAsync(CarriageTemplate carriageTemplate);
    Task<CarriageTemplate?> GetByIdAsync(Guid id);
    Task<List<CarriageTemplate>> GetByTrainTypeIdAsync(Guid trainTypeId);
}