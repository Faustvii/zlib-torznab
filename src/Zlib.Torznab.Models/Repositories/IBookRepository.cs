using Zlib.Torznab.Models.Archive;

namespace Zlib.Torznab.Models.Repositories;

public interface IBookRepository
{
    Task<Book?> GetBookFromMd5(string md5);
    Task<IReadOnlyList<Book>> GetLibgenFictionForIndex(int limit, DateTime newerThan, int skip);
    Task<IReadOnlyList<Book>> GetLibgenForIndex(int limit, DateTime DateTime, int skip);
    Task<IReadOnlyList<Book>> GetZlibForIndex(int limit, uint offset);
}
