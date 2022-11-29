namespace Zlib.Torznab.Services.Ipfs;

public interface IIpfsGateway
{
    Task<bool> DownloadFileAsync(
        string ipfsCid,
        string fileExtension,
        CancellationToken cancellationToken
    );
}
