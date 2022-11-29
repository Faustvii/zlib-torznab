using MonoTorrent.Client;
using MonoTorrent.TrackerServer;
using Zlib.Torznab.Presentation.API.Services;
using Zlib.Torznab.Services.Torrents;

namespace Zlib.Torznab.Presentation.API.HostedServices;

public class HostedTrackerService : IHostedService
{
    private TrackerServer _tracker = new();
    private readonly APITrackerListener _trackerListener;
    private readonly ITorrentService _torrentService;
    private readonly SemaphoreSlim _semaphore = new(1);

    public HostedTrackerService(APITrackerListener trackerListener, ITorrentService torrentService)
    {
        _trackerListener = trackerListener;
        _torrentService = torrentService;
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
        Console.WriteLine(
            $"{e.Peer.ClientApp.Client} {e.Peer.ClientAddress} {(e.Peer.HasCompleted ? "completed" : "wants")} {e.Torrent.Trackable.Name}"
        );
        var engine = _torrentService.GetEngine();
        if (
            e.Peer.HasCompleted
            && !string.Equals(
                e.Peer.PeerId.Text,
                engine.PeerId.Text,
                StringComparison.OrdinalIgnoreCase
            )
        )
        {
            Console.WriteLine(
                $"{e.Peer.ClientApp.Client} {e.Peer.ClientAddress} has completed the download, stopping seed"
            );
            var torrentManager = engine.Torrents.FirstOrDefault(
                x => x.InfoHashes.Contains(e.Torrent.Trackable.InfoHash)
            );
            if (torrentManager is not null)
            {
                await _semaphore.WaitAsync();
                if (torrentManager.State != TorrentState.Stopped)
                    await torrentManager.StopAsync();
                _semaphore.Release();
                await engine.RemoveAsync(torrentManager);
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
