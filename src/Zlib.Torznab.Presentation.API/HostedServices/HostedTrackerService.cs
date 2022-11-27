using MonoTorrent.Client;
using MonoTorrent.TrackerServer;
using Zlib.Torznab.Presentation.API.Services;

namespace Zlib.Torznab.Presentation.API.HostedServices;

public class HostedTrackerService : IHostedService
{
    private TrackerServer _tracker = new();
    private readonly APITrackerListener _trackerListener;
    private readonly ClientEngine _client;
    private readonly SemaphoreSlim _semaphore = new(1);

    public HostedTrackerService(APITrackerListener trackerListener, ClientEngine client)
    {
        _trackerListener = trackerListener;
        _client = client;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (_tracker.Disposed)
        {
            _tracker = new();
        }
        _tracker.AllowUnregisteredTorrents = true;
        _tracker.RegisterListener(_trackerListener);
        _tracker.PeerAnnounced += async (o, e) => await StopSeedingWhenDownloaded(o, e);

        return Task.CompletedTask;
    }

    private async Task StopSeedingWhenDownloaded(object? o, AnnounceEventArgs e)
    {
        if (
            e.Peer.HasCompleted
            && !string.Equals(
                e.Peer.PeerId.Text,
                _client.PeerId.Text,
                StringComparison.OrdinalIgnoreCase
            )
        )
        {
            var torrentManager = _client.Torrents.FirstOrDefault(
                x => x.InfoHashes.Contains(e.Torrent.Trackable.InfoHash)
            );
            if (torrentManager is not null)
            {
                await _semaphore.WaitAsync();
                if (torrentManager.State != TorrentState.Stopped)
                    await torrentManager.StopAsync();
                _semaphore.Release();
                await _client.RemoveAsync(torrentManager);
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        if (!_tracker.Disposed)
            _tracker.Dispose();
        return Task.CompletedTask;
    }
}
