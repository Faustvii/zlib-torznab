using Zlib.Torznab.Models.Archive;
using Zlib.Torznab.Models.Torznab;

namespace Zlib.Torznab.Services.Elastic;

public interface IElasticService
{
    Task IndexAllLibgen(CancellationToken cancellationToken);
    Task IndexLatestLibgen(CancellationToken cancellationToken);
    Task IndexAllLibgenFiction(CancellationToken cancellationToken);
    Task IndexLatestLibgenFiction(CancellationToken cancellationToken);
    Task IndexZlibrary();
    Task<IReadOnlyList<Book>> Search(TorznabRequest request);
    Task<Book?> GetBookByIPFS(string ipfsCid);
    Task<IReadOnlyList<Book>> RssFeed(int limit, int offset);
}
