using Ipfs.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MonoTorrent;
using MonoTorrent.Client;
using Zlib.Torznab.Models.Repositories;
using Zlib.Torznab.Models.Settings;

namespace Zlib.Torznab.Presentation.API.Controllers;

[ApiController]
[Route("[controller]")]
public class TorrentController : ControllerBase
{
    private readonly IFictionRepository _fictionRepository;
    private readonly ClientEngine _clientEngine;
    private readonly ApplicationSettings _applicationSettings;

    public TorrentController(
        IFictionRepository fictionRepository,
        ClientEngine clientEngine,
        IOptions<ApplicationSettings> optionsAccessor
    )
    {
        _fictionRepository = fictionRepository;
        _clientEngine = clientEngine;
        _applicationSettings = optionsAccessor.Value;
    }

    [HttpGet]
    [Route("{ipfs}")]
    [ProducesResponseType(typeof(FileContentResult), 200)]
    public async Task<IActionResult> Torrent(string ipfs)
    {
        var book = await _fictionRepository.GetFictionFromIpfs(ipfs);
        if (book is null)
            return NotFound();

        var ipfsClient = new IpfsClient(_applicationSettings.Ipfs.Gateway);
        var rootDirectory = _applicationSettings.Torrent.DownloadDirectory;
        var dir = Path.Combine(rootDirectory, ipfs);
        var fileName = $"{ipfs}.{book.Extension}";

        if (!System.IO.File.Exists(Path.Combine(dir, fileName)))
        {
            await using var fileContent = await ipfsClient.FileSystem.ReadFileAsync(
                ipfs,
                HttpContext.RequestAborted
            );
            Directory.CreateDirectory(dir);
            await using var fileStream = System.IO.File.Create(Path.Combine(dir, fileName));
            await fileContent.CopyToAsync(fileStream, HttpContext.RequestAborted);
            fileStream.Close();
        }

        var torrentCreator = new TorrentCreator
        {
            PieceLength = TorrentCreator.RecommendedPieceSize(
                new[] { Path.Combine(dir, fileName) }
            ),
            Comment = ipfs,
            Private = true,
            Announce = _applicationSettings.Torrent.TrackerUrl,
        };

        var torrentFileSource = new TorrentFileSource(dir)
        {
            TorrentName = $"{book.Author} - {book.Title} ({book.Year}) {book.Extension}",
        };

        var createResult = await torrentCreator.CreateAsync(
            torrentFileSource,
            HttpContext.RequestAborted
        );

        var torrentBytes = createResult.Encode();
        var torrent = MonoTorrent.Torrent.Load(createResult);
        var manager = await _clientEngine.AddAsync(torrent, dir);
        await manager.StartAsync();

        return File(torrentBytes, "application/x-bittorrent", $"{ipfs}.torrent");
    }
}
