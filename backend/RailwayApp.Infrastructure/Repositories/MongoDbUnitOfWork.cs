using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using RailwayApp.Domain.Interfaces;
using RailwayApp.Domain.Interfaces.IRepositories;

namespace RailwayApp.Infrastructure.Repositories;

public class MongoDbUnitOfWork : IUnitOfWork
{
    private readonly IMongoClient _mongoClient;
    private readonly IOptions<MongoDbSettings> _settings;
    private IClientSessionHandle _session;

    
    public MongoDbUnitOfWork(IMongoClient mongoClient, IOptions<MongoDbSettings> settings)
    {
        _mongoClient = mongoClient;
        _settings = settings;
    }
    
    public void Dispose()
    {
        _session?.Dispose();
    }
    
    public IClientSessionHandle CurrentSession => _session;

    private IUserAccountRepository _userAccounts;
    private ITicketRepository _tickets;
    private IStationRepository _stations;
    private IAbstractRouteRepository _abstractRoutes;
    private IAbstractRouteSegmentRepository _abstractRouteSegments;
    private ICarriageAvailabilityRepository _carriageAvailabilities;
    private ICarriageTemplateRepository _carriageTemplates;
    private IConcreteRouteSegmentRepository _concreteRouteSegments;
    private IConcreteRouteRepository _concreteRoutes;
    private ITrainRepository _trains;
    private ITrainTypeRepository _trainTypes;
    private ISeatLockRepository _seatLocks;
    
    public IUserAccountRepository UserAccounts => 
        _userAccounts ??= new MongoDbUserAccountRepository(_mongoClient, _settings);
    
    public ITicketRepository Tickets => 
        _tickets ??= new MongoDbTicketRepository(_mongoClient, _settings);
    
    public IStationRepository Stations => 
        _stations ??= new MongoDbStationRepository(_mongoClient, _settings);
    
    public IAbstractRouteRepository AbstractRoutes => 
        _abstractRoutes ??= new MongoDbAbstractRouteRepository(_mongoClient, _settings);
    
    public IAbstractRouteSegmentRepository AbstractRouteSegments => 
        _abstractRouteSegments ??= new MongoDbAbstractRouteSegmentRepository(_mongoClient, _settings);
    
    public ICarriageAvailabilityRepository CarriageAvailabilities => 
        _carriageAvailabilities ??= new MongoDbCarriageAvailabilityRepository(_mongoClient, _settings);
    
    public ICarriageTemplateRepository CarriageTemplates => 
        _carriageTemplates ??= new MongoDbCarriageTemplateRepository(_mongoClient, _settings);
    
    public IConcreteRouteSegmentRepository ConcreteRouteSegments => 
        _concreteRouteSegments ??= new MongoDbConcreteRouteSegmentRepository(_mongoClient, _settings);
    
    public IConcreteRouteRepository ConcreteRoutes => 
        _concreteRoutes ??= new MongoDbConcreteRouteRepository(_mongoClient, _settings);
    
    public ITrainRepository Trains => 
        _trains ??= new MongoDbTrainRepository(_mongoClient, _settings);
    
    public ITrainTypeRepository TrainTypes => 
        _trainTypes ??= new MongoDbTrainTypeRepository(_mongoClient, _settings);
    
    public ISeatLockRepository SeatLocks => 
        _seatLocks ??= new MongoDbSeatLockRepository(_mongoClient, _settings);
    
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task BeginTransactionAsync(TransactionOptions? options = null)
    {
        _session = await _mongoClient.StartSessionAsync();
        
        if (options != null)
        {
            _session.StartTransaction(options);
        }
        else
        {
            _session.StartTransaction(new TransactionOptions(
                ReadConcern.Snapshot,
                writeConcern: WriteConcern.WMajority));
        }
    }
    
    public async Task CommitTransactionAsync()
    {
        if (_session?.IsInTransaction == true)
        {
            await _session.CommitTransactionAsync();
        }
    }
    
    public async Task RollbackTransactionAsync()
    {
        if (_session?.IsInTransaction == true)
        {
            await _session.AbortTransactionAsync();
        }
    }
}