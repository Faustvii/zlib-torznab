using MonoTorrent.Client;

namespace Zlib.Torznab.Services.Torrents;

public interface ITorrentService
{
    ClientEngine GetEngine();
}
