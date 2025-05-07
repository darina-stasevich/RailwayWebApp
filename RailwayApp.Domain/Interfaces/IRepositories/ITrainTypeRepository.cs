using RailwayApp.Domain.Entities;

namespace RailwayApp.Domain.Interfaces.IRepositories;

public interface ITrainTypeRepository : IGlobalRepository<TrainType, Guid>
{
}