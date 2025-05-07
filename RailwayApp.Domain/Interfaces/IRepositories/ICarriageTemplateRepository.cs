using RailwayApp.Domain.Entities;

namespace RailwayApp.Domain.Interfaces.IRepositories;

public interface ICarriageTemplateRepository : IGlobalRepository<CarriageTemplate, Guid>
{
    Task<List<CarriageTemplate>?> GetByTrainTypeIdAsync(Guid trainTypeId);
}