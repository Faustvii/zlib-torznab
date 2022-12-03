using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using Zlib.Torznab.Models.Archive;
using Zlib.Torznab.Models.Settings;

namespace Zlib.Torznab.Services.Ipfs;

public partial class IpfsGateway : IIpfsGateway
{
    private readonly HttpClient _httpClient;
    private readonly ApplicationSettings _applicationSettings;

    [GeneratedRegex(
        "[\0/\0\"<>\\|\0\u0001\u0002\u0003\u0004\u0005\u0006\a\b\\t\\n\v\\f\\r\u000e\u000f\u0010\u0011\u0012\u0013\u0014\u0015\u0016\u0017\u0018\u0019\u001a\u001b\u001c\u001d\u001e\u001f:\\*\\?\\\\/]",
        RegexOptions.IgnoreCase,
        matchTimeoutMilliseconds: 5000
    )]
    private static partial Regex InvalidCharRegex();

    public IpfsGateway(HttpClient httpClient, IOptions<ApplicationSettings> options)
    {
        _httpClient = httpClient;
        _applicationSettings = options.Value;
    }

    public async Task<(bool Downloaded, string? FileName)> DownloadFileAsync(
        Book book,
        CancellationToken cancellationToken = default
    )
    {
        var rootDirectory = _applicationSettings.Torrent.DownloadDirectory;
        var dir = Path.Combine(rootDirectory, book.IpfsCid);
        var fileName = RemoveInvalidFileNameChars(book.FormattedTitle);
        var fullPath = Path.Combine(dir, fileName);
        try
        {
            if (!File.Exists(fullPath))
            {
                await using var fileContent = await _httpClient.GetStreamAsync(
                    $"{_applicationSettings.Ipfs.Gateway}{book.IpfsCid}?filename=book.{book.Extension}",
                    cancellationToken
                );
                Directory.CreateDirectory(dir);

                if (File.Exists(fullPath))
                    return (true, fileName);

                await using var fileStream = File.Create(fullPath);
                await fileContent.CopyToAsync(fileStream, cancellationToken);
                fileStream.Close();
            }
            return (true, fileName);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Download of IPFS was cancelled removing file that was created");
            if (File.Exists(fullPath))
                File.Delete(fullPath);
            return (false, null);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return (false, null);
        }
    }

    private static string RemoveInvalidFileNameChars(string input) =>
        InvalidCharRegex().Replace(input, string.Empty);
}
