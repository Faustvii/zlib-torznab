using System.Net;
using Microsoft.Extensions.Options;
using MonoTorrent.Client;
using TorrentSettings = Zlib.Torznab.Models.Settings.TorrentSettings;

namespace Zlib.Torznab.Services.Torrents;

public class TorrentService : ITorrentService
{
    private readonly TorrentSettings _torrentSettings;
    private readonly ClientEngine _clientEngine;

    public TorrentService(IOptions<TorrentSettings> options)
    {
        _torrentSettings = options.Value;
        var settingsBuilder = new EngineSettingsBuilder
        {
            ListenEndPoint = new IPEndPoint(IPAddress.Any, _torrentSettings.Port),
        };

        _clientEngine = new ClientEngine(settingsBuilder.ToSettings());
    }

    public ClientEngine GetEngine()
    {
        return _clientEngine;
    }
}
