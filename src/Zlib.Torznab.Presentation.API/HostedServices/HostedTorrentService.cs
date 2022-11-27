using MonoTorrent.Client;

namespace Zlib.Torznab.Presentation.API.HostedServices;

public class HostedTorrentService : IHostedService
{
    private readonly ClientEngine _client;

    public HostedTorrentService(ClientEngine client)
    {
        _client = client;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return _client.StartAllAsync();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return _client.StopAllAsync();
    }
}
