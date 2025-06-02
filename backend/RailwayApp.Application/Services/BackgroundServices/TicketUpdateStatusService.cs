using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using RailwayApp.Domain.Interfaces.IRepositories;

public class TicketUpdateStatusService : IHostedService, IDisposable
{
    private readonly ILogger<TicketUpdateStatusService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private Timer _timer;

    public TicketUpdateStatusService(
        ILogger<TicketUpdateStatusService> logger,
        IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    public Task StartAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("TicketUpdateStatusService is starting.");

        _timer = new Timer(async (state) => await DoWorkWrapperAsync(state, stoppingToken),
                           null,
                           TimeSpan.Zero,
                           TimeSpan.FromMinutes(1));
        
        _logger.LogInformation("TicketUpdateStatusService scheduled to run every minute.");
        return Task.CompletedTask;
    }

    private async Task DoWorkWrapperAsync(object state, CancellationToken stoppingToken)
    {
        _logger.LogInformation("TicketUpdateStatusService.DoWorkWrapperAsync started at {NowUtc}", DateTime.UtcNow);
        try
        {
            if (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Cancellation requested before starting work in TicketUpdateStatusService.");
                return;
            }
            await PerformTicketStatusUpdateAsync(stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception in TicketUpdateStatusService.DoWorkWrapperAsync.");
        }
        _logger.LogInformation("TicketUpdateStatusService.DoWorkWrapperAsync finished cycle at {NowUtc}", DateTime.UtcNow);
    }

    private async Task PerformTicketStatusUpdateAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("TicketUpdateStatusService: Starting to update ticket statuses.");

        using (var scope = _scopeFactory.CreateScope())
        {
            var ticketRepository = scope.ServiceProvider.GetRequiredService<ITicketRepository>();
            
            var currentUtcTime = DateTime.UtcNow;
            var secondsToSubtract = TimeSpan.FromSeconds(currentUtcTime.Second);
            var microSecondsToSubtract = TimeSpan.FromMilliseconds(currentUtcTime.Millisecond);
            
            var queryTimeUtc = currentUtcTime.Subtract(secondsToSubtract).Subtract(microSecondsToSubtract); 
            queryTimeUtc = queryTimeUtc.AddMinutes(1).AddMilliseconds(-1);

            _logger.LogDebug("TicketUpdateStatusService: Using query time <= {QueryTimeUtc} to find tickets.", queryTimeUtc);

            try
            {
                var ticketsToExpire = await ticketRepository.GetPayedTicketsPastDepartureAsync(queryTimeUtc);

                if (ticketsToExpire == null || !ticketsToExpire.Any())
                {
                    _logger.LogInformation("TicketUpdateStatusService: No 'Payed' tickets found with departure time <= {QueryTimeUtc}.", queryTimeUtc);
                    return;
                }

                var ticketIdsToExpire = ticketsToExpire.Select(t => t.Id).ToList();
                _logger.LogInformation("TicketUpdateStatusService: Found {Count} tickets to mark as Expired.",
                                       ticketIdsToExpire.Count);

                // Проверяем токен отмены перед длительной операцией
                if (stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("TicketUpdateStatusService: Cancellation requested before updating statuses.");
                    return;
                }

                long updatedCount = await ticketRepository.UpdateStatusesToExpiredAsync(ticketIdsToExpire);

                _logger.LogInformation("TicketUpdateStatusService: Successfully updated status to Expired for {UpdatedCount} out of {TotalCount} tickets.",
                                       updatedCount, ticketIdsToExpire.Count);

                if (updatedCount < ticketIdsToExpire.Count)
                {
                     _logger.LogWarning("TicketUpdateStatusService: Not all selected tickets were updated. Expected: {Expected}, Actual: {Actual}. This might indicate concurrent modifications or other issues.", 
                                        ticketIdsToExpire.Count, updatedCount);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("TicketUpdateStatusService: Ticket status update operation was canceled.");
            }
            catch (MongoException ex)
            {
                _logger.LogError(ex, "TicketUpdateStatusService: A MongoDB error occurred while updating ticket statuses. Query time was <= {QueryTimeUtc}.", queryTimeUtc);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TicketUpdateStatusService: An unexpected error occurred while updating ticket statuses. Query time was <= {QueryTimeUtc}.", queryTimeUtc);
            }
        }
        _logger.LogInformation("TicketUpdateStatusService: Finished updating ticket statuses for this run.");
    }

    public Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("TicketUpdateStatusService is stopping.");
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}