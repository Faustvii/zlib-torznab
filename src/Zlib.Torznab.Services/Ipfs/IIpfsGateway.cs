using Zlib.Torznab.Models.Archive;

namespace Zlib.Torznab.Services.Ipfs;

public interface IIpfsGateway
{
    Task<(bool Downloaded, string? FileName)> DownloadFileAsync(
        Book book,
        CancellationToken cancellationToken = default
    );
}
