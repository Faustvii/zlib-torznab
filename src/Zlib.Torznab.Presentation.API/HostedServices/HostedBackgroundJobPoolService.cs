using Zlib.Torznab.Models.Queues;

namespace Zlib.Torznab.Presentation.API.HostedServices;

public class HostedBackgroundJobPoolService : BackgroundService
{
    private readonly IBackgroundJobPool _jobPool;
    private readonly ILogger<HostedBackgroundJobPoolService> _logger;

    public HostedBackgroundJobPoolService(
        IBackgroundJobPool jobPool,
        ILogger<HostedBackgroundJobPoolService> logger
    )
    {
        _jobPool = jobPool;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var workItem = await _jobPool.DequeueAsync(stoppingToken);

                await workItem(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Prevent throwing if stoppingToken was signaled
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred executing background job item.");
            }
        }
    }
}
