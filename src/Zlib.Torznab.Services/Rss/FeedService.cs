using Zlib.Torznab.Models.Archive;
using Zlib.Torznab.Models.Repositories;

namespace Zlib.Torznab.Services.Rss;

public class FeedService : IFeedService
{
    private readonly IBookRepository _bookRepository;

    public FeedService(IBookRepository bookRepository)
    {
        _bookRepository = bookRepository;
    }

    public async Task<IReadOnlyList<Book>> GetFeed(int limit, int offset)
    {
        var items = await _bookRepository.GetBookFeed(DateTime.UtcNow.AddDays(-1), limit, offset);
        return items;
    }
}
