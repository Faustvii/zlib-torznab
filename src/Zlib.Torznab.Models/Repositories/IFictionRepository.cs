using Zlib.Torznab.Models.Archive;
using Zlib.Torznab.Models.Torznab;

namespace Zlib.Torznab.Models.Repositories;

public interface IFictionRepository
{
    Task<Book?> GetFictionFromIpfs(string ipfs);
    Task<IReadOnlyList<Book>> GetFictionsFromTorznabQuery(TorznabRequest request);
}
