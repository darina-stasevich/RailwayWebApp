using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Statuses;

namespace RailwayApp.Domain.Interfaces.IRepositories;

public interface ISeatLockRepository : IGenericRepository<SeatLock, Guid>
{
    Task<IEnumerable<SeatLock>> GetByRouteIdAsync(Guid ConcreteRouteId);
    Task<bool> UpdateStatusAsync(Guid seatLockId, SeatLockStatus status);

}