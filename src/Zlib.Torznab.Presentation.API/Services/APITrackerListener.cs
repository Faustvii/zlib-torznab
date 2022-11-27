using MonoTorrent.Connections;
using MonoTorrent.Connections.TrackerServer;

namespace Zlib.Torznab.Presentation.API.Services;

public class APITrackerListener : TrackerListener
{
    public override void Start()
    {
        RaiseStatusChanged(ListenerStatus.Listening);
    }

    public override void Stop()
    {
        RaiseStatusChanged(ListenerStatus.NotListening);
    }
}
