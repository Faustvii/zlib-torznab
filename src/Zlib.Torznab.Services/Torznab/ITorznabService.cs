

using Zlib.Torznab.Models.Torznab;
using Zlib.Torznab.Models.Torznab.Capabilities;
using Zlib.Torznab.Models.Torznab.Rss;

namespace Zlib.Torznab.Services.Torznab;

public interface ITorznabService
{
    Task<TorznabCapabilitiesResponse> GetCapabilities();
    Task<TorznabRss> GetFeed(TorznabRequest request);
}
