using System.Collections;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces.IRepositories;

public class ConcreteRouteGeneratorService(
    ILogger<ConcreteRouteGeneratorService> logger,
    IMongoClient mongoClient,
    IServiceScopeFactory scopeFactory)
    : IHostedService, IDisposable
{
    private const int GenerationAdvanceDays = 30;

    private Timer _timer;

    public void Dispose()
    {
        _timer?.Dispose();
    }

    public Task StartAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("ConcreteRouteGeneratorService is starting.");
        ScheduleNextRun(stoppingToken);
       
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("ConcreteRouteGeneratorService is stopping.");
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    private void ScheduleNextRun(CancellationToken stoppingToken, bool isRescheduling = false)
    {
        var now = DateTime.Now;
        var nextRunTime = now.Date.AddDays(1);
        var dueTime = nextRunTime - now;
        if (dueTime <= TimeSpan.Zero)
            dueTime = TimeSpan.Zero;

        if (_timer == null)
        {
            _timer = new Timer(async state => await DoWorkWrapperAsync(state, stoppingToken), null,
                Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
            logger.LogInformation("Timer created.");
        }

        _timer.Change(dueTime, Timeout.InfiniteTimeSpan);

        if (isRescheduling)
            logger.LogInformation(
                "ConcreteRouteGeneratorService rescheduled. Next run at {NextRunDateTimeUtc} (in {DueTime})",
                nextRunTime, dueTime);
        else
            logger.LogInformation(
                "ConcreteRouteGeneratorService scheduled. First run at {NextRunDateTimeUtc} (in {DueTime})",
                nextRunTime, dueTime);
    }

    private async Task DoWorkWrapperAsync(object state, CancellationToken stoppingToken)
    {
        logger.LogInformation("ConcreteRouteGeneratorService.DoWorkWrapperAsync started at {Now}", DateTime.Now);
        try
        {
            if (stoppingToken.IsCancellationRequested)
            {
                logger.LogInformation("Cancellation requested before starting actual work in DoWorkWrapperAsync.");
                return;
            }

            await DoWorkAsync(state, stoppingToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception in DoWorkAsync execution.");
        }
        finally
        {
            if (!stoppingToken.IsCancellationRequested)
            {
                logger.LogInformation("Work finished. Rescheduling next run.");
                ScheduleNextRun(stoppingToken, true);
            }
            else
            {
                logger.LogInformation("Cancellation requested. Not rescheduling next run.");
            }
        }

        logger.LogInformation("ConcreteRouteGeneratorService.DoWorkWrapperAsync finished at {Now}", DateTime.Now);
    }

    private async Task DoWorkAsync(object state, CancellationToken stoppingToken)
    {
        var today = DateTime.Now.Date;
        logger.LogInformation("ConcreteRouteGeneratorService.DoWorkAsync is starting.");
        using (var scope = scopeFactory.CreateScope())
        {
            var abstractRouteRepository =
                scope.ServiceProvider.GetRequiredService<IAbstractRouteRepository>();
            var abstractRouteSegmentRepository =
                scope.ServiceProvider.GetRequiredService<IAbstractRouteSegmentRepository>();
            var concreteRouteRepository =
                scope.ServiceProvider.GetRequiredService<IConcreteRouteRepository>();
            var concreteRouteSegmentRepository =
                scope.ServiceProvider.GetRequiredService<IConcreteRouteSegmentRepository>();
            var trainRepository = scope.ServiceProvider.GetRequiredService<ITrainRepository>();
            var carriageTemplateRepository =
                scope.ServiceProvider.GetRequiredService<ICarriageTemplateRepository>();
            var carriageAvailabilityRepository =
                scope.ServiceProvider.GetRequiredService<ICarriageAvailabilityRepository>();
            try
            {
                logger.LogInformation(
                    "ConcreteRouteGeneratorService is running at {RunTime} for target generation date {TargetDate}",
                    DateTime.Now, today.AddDays(GenerationAdvanceDays));
                await GenerateRoutesForDayAsync(today.AddDays(GenerationAdvanceDays), stoppingToken,
                    abstractRouteRepository, abstractRouteSegmentRepository, concreteRouteRepository,
                    concreteRouteSegmentRepository, trainRepository, carriageTemplateRepository,
                    carriageAvailabilityRepository);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred in ConcreteRouteGeneratorService.DoWorkAsync");
            }
        }
    }

    private async Task GenerateRoutesForDayAsync(DateTime targetDepartureDate, CancellationToken stoppingToken,
        IAbstractRouteRepository abstractRouteRepository,
        IAbstractRouteSegmentRepository abstractRouteSegmentRepository,
        IConcreteRouteRepository concreteRouteRepository,
        IConcreteRouteSegmentRepository concreteRouteSegmentRepository, ITrainRepository trainRepository,
        ICarriageTemplateRepository carriageTemplateRepository,
        ICarriageAvailabilityRepository carriageAvailabilityRepository)
    {
        logger.LogInformation("Generating concrete routes for departure date: {TargetDepartureDateUtc}",
            targetDepartureDate.ToString("yyyy-MM-dd"));

        var activeAbstractRoutes = await abstractRouteRepository.GetActiveRoutes();

        foreach (var abstractRoute in activeAbstractRoutes)
        {
            if (stoppingToken.IsCancellationRequested)
            {
                logger.LogInformation("Cancellation requested, stopping route generation.");
                return;
            }

            var shouldGenerate = ShouldGenerateForDate(abstractRoute, targetDepartureDate);

            if (shouldGenerate)
            {
                var routesInDate = await concreteRouteRepository.GetConcreteRoutesInDate(targetDepartureDate.Date,
                    targetDepartureDate.Date.AddDays(1));
                var exists = routesInDate.FirstOrDefault(r => r.AbstractRouteId == abstractRoute.Id);
                if (exists != null)
                {
                    logger.LogInformation(
                        "ConcreteRoute for AbstractRouteId {AbstractRouteId} at {DepartureDateTimeUtc} already exists. Skipping.",
                        abstractRoute.Id, targetDepartureDate);
                    continue;
                }

                await CreateAndSaveConcreteRouteWithTransactionAsync(abstractRoute, targetDepartureDate, stoppingToken,
                    abstractRouteSegmentRepository, concreteRouteRepository, concreteRouteSegmentRepository,
                    trainRepository, carriageTemplateRepository, carriageAvailabilityRepository);
            }
        }

        logger.LogInformation("Finished generating concrete routes for departure date: {TargetDepartureDateUtc}",
            targetDepartureDate.ToString("yyyy-MM-dd"));
    }

    private bool ShouldGenerateForDate(AbstractRoute abstractRoute, DateTime targetDate)
    {
        return abstractRoute.ActiveDays.Contains(targetDate.DayOfWeek);
    }

    private async Task CreateAndSaveConcreteRouteWithTransactionAsync(
        AbstractRoute abstractRoute,
        DateTime departureDate,
        CancellationToken stoppingToken,
        IAbstractRouteSegmentRepository abstractRouteSegmentRepository,
        IConcreteRouteRepository concreteRouteRepository,
        IConcreteRouteSegmentRepository concreteRouteSegmentRepository, ITrainRepository trainRepository,
        ICarriageTemplateRepository carriageTemplateRepository,
        ICarriageAvailabilityRepository carriageAvailabilityRepository)
    {
        var localDepartureDateTime = departureDate.Add(abstractRoute.DepartureTime);

        // 1. get abstract segments
        var abstractSegments =
            (await abstractRouteSegmentRepository.GetAbstractSegmentsByRouteIdAsync(abstractRoute.Id))
            .OrderBy(s => s.SegmentNumber).ToList();
        if (abstractSegments == null || abstractSegments.Count == 0)
        {
            logger.LogWarning("Abstract route {routeId} has no abstract segments", abstractRoute.Id);
            return;
        }

        // 2. get train for route
        var train = await trainRepository.GetByIdAsync(abstractRoute.TrainNumber);
        if (train == null)
        {
            logger.LogWarning("Train {TrainNumber} not found for AbstractRoute {AbstractRouteId}. Skipping.",
                abstractRoute.TrainNumber, abstractRoute.Id);
            return;
        }

        // 3. get carriage templates for train
        var carriageTemplates = (await carriageTemplateRepository.GetByTrainTypeIdAsync(train.TrainTypeId))
            ?.OrderBy(c => c.CarriageNumber).ToList();
        if (carriageTemplates == null || !carriageTemplates.Any())
        {
            logger.LogWarning("No carriage templates for TrainType {TrainTypeId} (Train: {TrainNumber}). Skipping.",
                train.TrainTypeId, abstractRoute.TrainNumber);
            return;
        }

        // create route, route segments, carriage availabilities
        using (var session = await mongoClient.StartSessionAsync(cancellationToken: stoppingToken))
        {
            session.StartTransaction(new TransactionOptions(
                ReadConcern.Snapshot,
                writeConcern: WriteConcern.WMajority));
            try
            {
                // add concrete route
                var concreteRoute = new ConcreteRoute
                {
                    AbstractRouteId = abstractRoute.Id,
                    RouteDepartureDate = localDepartureDateTime
                };
                var concreteRouteId = await concreteRouteRepository.AddAsync(concreteRoute, session);
                logger.LogInformation("Created ConcreteRoute {ConcreteRouteId} in transaction.", concreteRoute.Id);

                // add concrete route segments
                var concreteSegments = new List<ConcreteRouteSegment>();
                foreach (var abstractSegment in abstractSegments)
                {
                    if (stoppingToken.IsCancellationRequested)
                    {
                        await session.AbortTransactionAsync(stoppingToken);
                        logger.LogWarning("Transaction aborted due to cancellation request.");
                        return;
                    }

                    var concreteRouteSegment = new ConcreteRouteSegment
                    {
                        AbstractSegmentId = abstractSegment.Id,
                        ConcreteDepartureDate = departureDate.Add(abstractSegment.FromTime),
                        ConcreteArrivalDate = departureDate.Add(abstractSegment.ToTime),
                        ConcreteRouteId = concreteRouteId,
                        FromStationId = abstractSegment.FromStationId,
                        ToStationId = abstractSegment.ToStationId,
                        SegmentNumber = abstractSegment.SegmentNumber
                    };

                    concreteSegments.Add(concreteRouteSegment);
                }

                await concreteRouteSegmentRepository.AddRangeAsync(concreteSegments, session);

                // add carriage availabilities
                var carriageAvailabilities = new List<CarriageAvailability>();
                logger.LogInformation("concrete segments amount is {amount}", concreteSegments.Count);
                logger.LogInformation("carriage templates amount is {amount}", carriageTemplates.Count);
                foreach (var concreteSegment in concreteSegments)
                {
                    if (stoppingToken.IsCancellationRequested)
                    {
                        await session.AbortTransactionAsync(stoppingToken);
                        logger.LogWarning("Transaction aborted due to cancellation request.");
                        return;
                    }

                    foreach (var carriageTemplate in carriageTemplates)
                    {
                        var carriageAvailability = new CarriageAvailability
                        {
                            CarriageTemplateId = carriageTemplate.Id,
                            ConcreteRouteSegmentId = concreteSegment.Id,
                            OccupiedSeats = new BitArray(carriageTemplate.TotalSeats, true)
                        };
                        carriageAvailabilities.Add(carriageAvailability);
                    }
                }

                logger.LogInformation("noe got {amount} carriage availiabilities", carriageAvailabilities.Count);

                await carriageAvailabilityRepository.AddRangeAsync(carriageAvailabilities, session);

                if (stoppingToken.IsCancellationRequested)
                {
                    await session.AbortTransactionAsync(stoppingToken);
                    logger.LogWarning("Transaction aborted due to cancellation request.");
                    return;
                }

                await session.CommitTransactionAsync(stoppingToken);
                logger.LogInformation(
                    "Successfully committed transaction for ConcreteRoute {ConcreteRouteId} (AbstractRouteId: {AbstractRouteId}, Train: {TrainNumber}).",
                    concreteRoute.Id, abstractRoute.Id, abstractRoute.TrainNumber);
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "Error during transaction for AbstractRoute {AbstractRouteId} (Train: {TrainNumber}). Aborting transaction.",
                    abstractRoute.Id, abstractRoute.TrainNumber);
                if (session.IsInTransaction) await session.AbortTransactionAsync(CancellationToken.None);
            }
        }
    }
}