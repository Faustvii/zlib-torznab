using Zlib.Torznab.Models.Archive;

namespace Zlib.Torznab.Services.Ipfs;

public interface IIpfsGateway
{
    Task<bool> DownloadFileAsync(Book book, CancellationToken cancellationToken);
}
