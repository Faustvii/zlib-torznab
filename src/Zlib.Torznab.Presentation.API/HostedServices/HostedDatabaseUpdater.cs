using Zlib.Torznab.Services.Metadata;

namespace Zlib.Torznab.Presentation.API.HostedServices;

public sealed class HostedDatabaseUpdater : IHostedService, IDisposable
{
    private readonly ILogger<HostedDatabaseUpdater> _logger;
    private Timer? _timer = null;

    public IServiceProvider Services { get; }

    public HostedDatabaseUpdater(IServiceProvider services, ILogger<HostedDatabaseUpdater> logger)
    {
        Services = services;
        _logger = logger;
    }

    private async void ExecuteAsync(object? state)
    {
        if (state is CancellationToken ct)
        {
            await using var scope = Services.CreateAsyncScope();
            var metadataService = scope.ServiceProvider.GetRequiredService<IMetadataService>();
            await metadataService.ImportLatestData(ct);
        }
        else
        {
            _logger.LogError("Object state was not a cancellation token");
        }
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Scheduled database updater is running.");

        _timer = new Timer(
            ExecuteAsync,
            state: cancellationToken,
            TimeSpan.Zero,
            TimeSpan.FromHours(1)
        );
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Scheduled database updater is stopping.");
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}
