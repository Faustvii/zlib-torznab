using Zlib.Torznab.Services.Elastic;

namespace Zlib.Torznab.Presentation.API.HostedServices;

public sealed class HostedIndexUpdater : IHostedService, IDisposable
{
    private readonly ILogger<HostedIndexUpdater> _logger;
    private Timer? _timer = null;
    private readonly SemaphoreSlim _semaphore = new(1);

    public IServiceProvider Services { get; }

    public HostedIndexUpdater(IServiceProvider services, ILogger<HostedIndexUpdater> logger)
    {
        Services = services;
        _logger = logger;
    }

    private async void ExecuteAsync(object? state)
    {
        try
        {
            if (state is CancellationToken ct)
            {
                await _semaphore.WaitAsync();
                await using var scope = Services.CreateAsyncScope();
                var elasticService = scope.ServiceProvider.GetRequiredService<IElasticService>();
                await elasticService.IndexLatestLibgen(ct);
                await elasticService.IndexLatestLibgenFiction(ct);
            }
            else
            {
                _logger.LogError("Object state was not a cancellation token");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during {Service} execution", nameof(HostedIndexUpdater));
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Scheduled index updater is running.");

        _timer = new Timer(
            ExecuteAsync,
            state: cancellationToken,
            TimeSpan.Zero,
            TimeSpan.FromMinutes(15)
        );
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Scheduled index updater is stopping.");
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}
