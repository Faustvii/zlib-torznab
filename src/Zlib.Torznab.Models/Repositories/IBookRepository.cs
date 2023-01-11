using Zlib.Torznab.Models.Archive;

namespace Zlib.Torznab.Models.Repositories;

public interface IBookRepository
{
    Task<Book?> GetBookFromMd5(string md5);
    Task<IReadOnlyList<Book>> GetAllLibgenForIndex(int limit, uint largerThanId);
    Task<IReadOnlyList<Book>> GetAllLibgenFictionForIndex(int limit, uint largerThanId);
    Task<IReadOnlyList<Book>> GetLatestLibgenFictionForIndex(int limit, int skip);
    Task<IReadOnlyList<Book>> GetLatestLibgenForIndex(int limit, int skip);
    Task<IReadOnlyList<Book>> GetZlibForIndex(int limit, uint offset);
}
