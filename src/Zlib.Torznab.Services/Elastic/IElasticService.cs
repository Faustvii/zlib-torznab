using Zlib.Torznab.Models.Archive;
using Zlib.Torznab.Models.Torznab;

namespace Zlib.Torznab.Services.Elastic;

public interface IElasticService
{
    Task IndexLatestLibgen(CancellationToken cancellationToken);
    Task IndexLatestLibgenFiction(CancellationToken cancellationToken);
    Task IndexZlibrary();
    Task<IReadOnlyList<Book>> Search(TorznabRequest request);
    Task<IReadOnlyList<Book>> RssFeed(int limit, int offset);
}
