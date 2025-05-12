using MongoDB.Driver;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Statuses;

namespace RailwayApp.Domain.Interfaces.IRepositories;

public interface ISeatLockRepository : IGenericRepository<SeatLock, Guid>
{
    Task<IEnumerable<SeatLock>> GetByRouteIdAsync(Guid concreteRouteId, IClientSessionHandle? session = null);
    Task<IEnumerable<SeatLock>> GetByUserAccountIdAsync(Guid userAccountId);
    Task<bool> UpdateStatusAsync(Guid seatLockId, SeatLockStatus status, IClientSessionHandle? session = null);
    Task<bool> PrepareForProcessingAsync(Guid seatLockId, DateTime newExpirationTime,
        SeatLockStatus newStatus, SeatLockStatus expectedCurrentStatus, IClientSessionHandle session);

}