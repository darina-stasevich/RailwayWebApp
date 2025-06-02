using System.Transactions;
using MongoDB.Driver;
using TransactionOptions = MongoDB.Driver.TransactionOptions;

namespace RailwayApp.Domain.Interfaces.IRepositories;
public interface IUnitOfWork : IDisposable
{
    IUserAccountRepository UserAccounts { get; }
    ITicketRepository Tickets { get; }
    IStationRepository Stations { get; }
    IAbstractRouteRepository AbstractRoutes { get; }
    IAbstractRouteSegmentRepository AbstractRouteSegments { get; }
    ICarriageAvailabilityRepository CarriageAvailabilities { get; }
    ICarriageTemplateRepository CarriageTemplates { get; }
    IConcreteRouteSegmentRepository ConcreteRouteSegments { get; }
    IConcreteRouteRepository ConcreteRoutes { get; }
    ITrainRepository Trains { get; }
    ITrainTypeRepository TrainTypes { get; }
    ISeatLockRepository SeatLocks { get; }
    
    IClientSessionHandle CurrentSession { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(TransactionOptions? options = null);
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}