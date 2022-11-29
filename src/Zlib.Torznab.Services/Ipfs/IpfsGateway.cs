using Microsoft.Extensions.Options;
using Zlib.Torznab.Models.Settings;

namespace Zlib.Torznab.Services.Ipfs;

public class IpfsGateway : IIpfsGateway
{
    private readonly HttpClient _httpClient;
    private readonly ApplicationSettings _applicationSettings;

    public IpfsGateway(HttpClient httpClient, IOptions<ApplicationSettings> options)
    {
        _httpClient = httpClient;
        _applicationSettings = options.Value;
    }

    public async Task<bool> DownloadFileAsync(
        string ipfsCid,
        string fileExtension,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var rootDirectory = _applicationSettings.Torrent.DownloadDirectory;
            var dir = Path.Combine(rootDirectory, ipfsCid);
            var fileName = $"{ipfsCid}.{fileExtension}";

            if (!File.Exists(Path.Combine(dir, fileName)))
            {
                await using var fileContent = await _httpClient.GetStreamAsync(
                    $"{_applicationSettings.Ipfs.Gateway}{ipfsCid}?filename=book.{fileExtension}",
                    cancellationToken
                );
                Directory.CreateDirectory(dir);
                await using var fileStream = File.Create(Path.Combine(dir, fileName));
                await fileContent.CopyToAsync(fileStream, cancellationToken);
                fileStream.Close();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
        return true;
    }
}
