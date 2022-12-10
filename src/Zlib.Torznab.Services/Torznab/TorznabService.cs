using System.Globalization;
using Microsoft.Extensions.Options;
using Zlib.Torznab.Models.Archive;
using Zlib.Torznab.Models.Queues;
using Zlib.Torznab.Models.Repositories;
using Zlib.Torznab.Models.Settings;
using Zlib.Torznab.Models.Torznab;
using Zlib.Torznab.Models.Torznab.Capabilities;
using Zlib.Torznab.Models.Torznab.Rss;
using Zlib.Torznab.Services.Elastic;
using Zlib.Torznab.Services.Ipfs;

namespace Zlib.Torznab.Services.Torznab;

public class TorznabService : ITorznabService
{
    private readonly ApplicationSettings _applicationSettings;
    private readonly IBackgroundJobPool _jobPool;
    private readonly IIpfsGateway _ipfsGateway;
    private readonly IMetadataRepository _metadataRepository;
    private readonly IElasticService _elasticService;

    public TorznabService(
        IOptions<ApplicationSettings> optionsAccessor,
        IBackgroundJobPool jobPool,
        IIpfsGateway ipfsGateway,
        IMetadataRepository metadataRepository,
        IElasticService elasticService
    )
    {
        _applicationSettings = optionsAccessor.Value;
        _jobPool = jobPool;
        _ipfsGateway = ipfsGateway;
        _metadataRepository = metadataRepository;
        _elasticService = elasticService;
    }

    public Task<TorznabCapabilitiesResponse> GetCapabilities()
    {
        var capabilities = new TorznabCapabilitiesResponse
        {
            Searching = new TorznabCapsSearching
            {
                BookSearch = new TorznabCapsSearchingParams
                {
                    SupportedParams = string.Join(",", Enum.GetValues<BookSearchParam>())
                        .ToLowerInvariant(),
                },
            },
            Categories = new TorznabCategories
            {
                Category = new List<TorznabCategory>
                {
                    new TorznabCategory
                    {
                        Id = 7000,
                        Name = "Books",
                        Subcat = new List<TorznabSubCategory>
                        {
                            new TorznabSubCategory { Id = 7020, Name = "Books/EBook", },
                        },
                    },
                },
            },
        };
        return Task.FromResult(capabilities);
    }

    public async Task<TorznabRss> GetFeed(TorznabRequest request)
    {
        var metadata = await _metadataRepository.GetMetadata();
        var items = request.IsRSS
            ? await _elasticService.RssFeed(request.Limit, request.Offset)
            : await _elasticService.Search(request);

        var result = new TorznabRss
        {
            Channel = new Channel
            {
                LastBuildDate = metadata.LatestUpdate,
                Response = new Response { Offset = request.Offset, },
                Item = items.Select(MapItem).ToList(),
            },
        };

        if (request.Offset > 200 && !string.IsNullOrWhiteSpace(request.Query))
            result.Channel.Item = new();

        if (items.Count == 1 && !request.IsRSS)
        {
            await _jobPool.QueueBackgroundWorkItemAsync((ct) => QueueIPFSDownload(items[0], ct));
        }

        return result;
    }

    private async ValueTask QueueIPFSDownload(Book book, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Queuing download of {book.Title} {book.Author} - {book.IpfsCid}");
        await _ipfsGateway.DownloadFileAsync(book, cancellationToken);
        Console.WriteLine($"Finished download of {book.Title} {book.Author} - {book.IpfsCid}");
    }

    private Item MapItem(Book x)
    {
        return new Item
        {
            Title = x.FormattedTitle,
            TorznabGuid = new TorznabGuid { IsPermaLink = false, Text = x.IpfsCid, },
            PubDate = x.TimeAdded.ToString("r", CultureInfo.InvariantCulture),
            Source = new Source { Url = $"{_applicationSettings.Torznab.SourceUrlBase}{x.Md5}" },
            Attr = new List<Attr>
            {
                new Attr { Name = "files", Value = "1" },
                new Attr
                {
                    Name = "size",
                    Value = x.Filesize.ToString(CultureInfo.InvariantCulture),
                },
                new Attr { Name = "booktitle", Value = x.Title, },
                new Attr { Name = "author", Value = x.Author, },
                new Attr { Name = "pages", Value = x.Pages, },
                new Attr { Name = "year", Value = x.Year, },
                new Attr { Name = "seeders", Value = "100", },
                new Attr { Name = "leechers", Value = "0", },
                new Attr { Name = "peers", Value = "100", },
                new Attr { Name = "category", Value = "7000", },
                new Attr { Name = "category", Value = "7020", },
                new Attr { Name = "downloadvolumefactor", Value = "0", },
                new Attr { Name = "language", Value = x.Language, },
                new Attr { Name = "type", Value = "book", },
                new Attr { Name = "publisher", Value = x.Publisher, },
            },
            Enclosure = new List<Enclosure>
            {
                new Enclosure
                {
                    Url = $"{_applicationSettings.Torznab.TorrentDownloadBase}{x.Md5}",
                    Type = "application/x-bittorrent",
                    Length = (int)x.Filesize,
                },
            },
        };
    }
}
