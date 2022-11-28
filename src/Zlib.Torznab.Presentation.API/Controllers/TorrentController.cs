using Ipfs.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MonoTorrent;
using Zlib.Torznab.Models.Repositories;
using Zlib.Torznab.Models.Settings;
using Zlib.Torznab.Services.Torrents;

namespace Zlib.Torznab.Presentation.API.Controllers;

[ApiController]
[Route("[controller]")]
public class TorrentController : ControllerBase
{
    private readonly IBookRepository _bookRepository;
    private readonly ITorrentService _torrentService;
    private readonly ApplicationSettings _applicationSettings;

    public TorrentController(
        IBookRepository bookRepository,
        ITorrentService torrentService,
        IOptions<ApplicationSettings> optionsAccessor
    )
    {
        _bookRepository = bookRepository;
        _torrentService = torrentService;
        _applicationSettings = optionsAccessor.Value;
    }

    [HttpGet]
    [Route("{md5}")]
    [ProducesResponseType(typeof(FileContentResult), 200)]
    public async Task<IActionResult> Torrent(string md5)
    {
        var book = await _bookRepository.GetBookFromMd5(md5);
        if (book is null)
            return NotFound();

        var ipfsClient = new IpfsClient(_applicationSettings.Ipfs.Gateway);
        var rootDirectory = _applicationSettings.Torrent.DownloadDirectory;
        var dir = Path.Combine(rootDirectory, book.IpfsCid);
        var fileName = $"{book.IpfsCid}.{book.Extension}";

        if (!System.IO.File.Exists(Path.Combine(dir, fileName)))
        {
            await using var fileContent = await ipfsClient.FileSystem.ReadFileAsync(
                book.IpfsCid,
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
            Comment = book.IpfsCid,
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
        var manager = await _torrentService.GetEngine().AddAsync(torrent, dir);
        await manager.StartAsync();

        return File(torrentBytes, "application/x-bittorrent", $"{md5}.torrent");
    }
}
