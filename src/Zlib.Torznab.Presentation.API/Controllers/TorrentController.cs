using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MonoTorrent;
using Zlib.Torznab.Models.Repositories;
using Zlib.Torznab.Models.Settings;
using Zlib.Torznab.Services.Ipfs;
using Zlib.Torznab.Services.Torrents;

namespace Zlib.Torznab.Presentation.API.Controllers;

[ApiController]
[Route("[controller]")]
public class TorrentController : ControllerBase
{
    private readonly IBookRepository _bookRepository;
    private readonly ITorrentService _torrentService;
    private readonly IIpfsGateway _ipfsGateway;
    private readonly ApplicationSettings _applicationSettings;

    public TorrentController(
        IBookRepository bookRepository,
        ITorrentService torrentService,
        IOptions<ApplicationSettings> optionsAccessor,
        IIpfsGateway ipfsGateway
    )
    {
        _bookRepository = bookRepository;
        _torrentService = torrentService;
        _ipfsGateway = ipfsGateway;
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

        var rootDirectory = _applicationSettings.Torrent.DownloadDirectory;
        var dir = Path.Combine(rootDirectory, book.IpfsCid);
        var fileName = $"{book.IpfsCid}.{book.Extension}";

        if (
            !await _ipfsGateway.DownloadFileAsync(
                book.IpfsCid,
                book.Extension,
                HttpContext.RequestAborted
            )
        )
            return NotFound();

        var torrentCreator = new TorrentCreator
        {
            PieceLength = TorrentCreator.RecommendedPieceSize(
                new[] { Path.Combine(dir, fileName) }
            ),
            Comment = $"{book.IpfsCid} - MD5 {book.Md5}",
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
        try
        {
            var engine = _torrentService.GetEngine();
            var torrent = MonoTorrent.Torrent.Load(createResult);
            var manager =
                engine.Torrents.FirstOrDefault(x => x.InfoHashes == torrent.InfoHashes)
                ?? await engine.AddAsync(torrent, dir);
            manager.ConnectionAttemptFailed += (o, e) =>
                Console.WriteLine($"connection failed {e.Peer.PeerId} because of {e.Reason}");
            manager.PeersFound += (o, e) =>
                Console.WriteLine(
                    $"We found {e.NewPeers} new peers {e.ExistingPeers} existing peers"
                );
            manager.TorrentStateChanged += (o, e) =>
                Console.WriteLine($"{e.TorrentManager.Name} has new state {e.NewState}");
            manager.PeerConnected += (o, e) =>
                Console.WriteLine($"{e.Peer.ClientApp} connected with {e.Direction} direction");
            manager.PeerDisconnected += (o, e) =>
                Console.WriteLine($"{e.Peer.ClientApp} disconnected");
            await manager.StartAsync();
        }
        catch (TorrentException ex)
            when (ex.Message.Contains("already been registered", StringComparison.OrdinalIgnoreCase)
            )
        {
            //ignore
        }

        return File(torrentBytes, "application/x-bittorrent", $"{md5}.torrent");
    }
}
