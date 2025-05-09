using RailwayApp.Domain.Entities;

namespace RailwayApp.Domain.Interfaces.IRepositories;

public interface ITrainRepository : IGenericRepository<Train, string>
{
}