using Zlib.Torznab.Models.Archive;
using Zlib.Torznab.Models.Torznab;

namespace Zlib.Torznab.Models.Repositories;

public interface IBookRepository
{
    Task<Book?> GetBookFromMd5(string md5);
    Task<IReadOnlyList<Book>> GetBooksFromTorznabQuery(TorznabRequest request);
    Task<IReadOnlyList<Book>> GetBookFeed(DateTime olderThan, int limit, int offset);
}
