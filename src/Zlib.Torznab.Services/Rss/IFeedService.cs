using Zlib.Torznab.Models.Archive;

namespace Zlib.Torznab.Services.Rss;

public interface IFeedService
{
    Task<IReadOnlyList<Book>> GetFeed(int limit, int offset);
}
