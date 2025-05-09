using MongoDB.Driver;
using RailwayApp.Domain.Entities;

namespace RailwayApp.Domain.Interfaces.IRepositories;

public interface ICarriageTemplateRepository : IGenericRepository<CarriageTemplate, Guid>
{
    Task<IEnumerable<CarriageTemplate>?> GetByTrainTypeIdAsync(Guid trainTypeId, IClientSessionHandle? session = null);
}