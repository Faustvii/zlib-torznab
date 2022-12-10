using Microsoft.Extensions.Logging;
using Nest;
using Zlib.Torznab.Models.Archive;
using Zlib.Torznab.Models.Repositories;
using Zlib.Torznab.Models.Torznab;

namespace Zlib.Torznab.Services.Elastic;

public class ElasticService : IElasticService
{
    private readonly IBookRepository _bookRepository;
    private readonly IMetadataRepository _metadataRepository;
    private readonly IElasticClient _elasticClient;
    private readonly ILogger<ElasticService> _logger;

    public ElasticService(
        IBookRepository bookRepository,
        IMetadataRepository metadataRepository,
        IElasticClient elasticClient,
        ILogger<ElasticService> logger
    )
    {
        _bookRepository = bookRepository;
        _metadataRepository = metadataRepository;
        _elasticClient = elasticClient;
        _logger = logger;
    }

    public async Task<IReadOnlyList<Book>> Search(TorznabRequest request)
    {
        var searchResponse = await _elasticClient.SearchAsync<ElasticBook>(
            s =>
                s.Query(
                        q =>
                            q.Match(m => m.Field(f => f.Author).Query(request.Author))
                            && q.Match(m => m.Field(f => f.Title).Query(request.Title))
                    )
                    .Sort(x => x.Descending(SortSpecialField.Score))
                    .Take(request.Limit)
                    .Skip(request.Offset)
        );

        return searchResponse.Documents.Select(Map).ToList();
    }

    private static Book Map(ElasticBook x)
    {
        return new Book
        {
            Author = x.Author,
            Extension = x.Extension,
            Filesize = x.Filesize,
            Identifier = x.Identifier,
            IpfsCid = x.IpfsCid,
            Md5 = x.Md5,
            Language = x.Language,
            Locator = x.Locator,
            Pages = x.Pages,
            Publisher = x.Publisher,
            TimeModified = x.LatestChange,
            TimeAdded = x.LatestChange,
            Title = x.Title,
            Year = x.Year,
        };
    }

    public async Task<IReadOnlyList<Book>> RssFeed(int limit, int offset)
    {
        var dateLimit = DateTime.UtcNow.AddDays(-1);
        var latestBooks = await _elasticClient.SearchAsync<ElasticBook>(
            s =>
                s.Query(
                        q =>
                            q.DateRange(
                                x => x.Field(b => b.LatestChange).LessThanOrEquals(dateLimit)
                            )
                    )
                    .Sort(x => x.Descending(b => b.LatestChange))
                    .Take(limit)
                    .Skip(offset)
        );

        return latestBooks.Documents.Select(Map).ToList();
    }

    public async Task IndexLatestLibgen(CancellationToken cancellationToken)
    {
        var take = 10000;
        var metadata = await _metadataRepository.GetMetadata();
        var newerThan = metadata.LastestLibgenEntry;
        var previousNewerThan = (DateTime?)null;
        var skip = 0;
        var latestDate = (DateTime?)null;
        var bookCounter = 0;

        var books = await _bookRepository.GetLibgenForIndex(take, newerThan, skip);
        while (books.Any())
        {
            var mappedBooks = books.Select(x => Map(x, "Libgen")).ToList();
            var response = await _elasticClient.IndexManyAsync(
                mappedBooks,
                cancellationToken: cancellationToken
            );
            if (previousNewerThan == newerThan)
                skip += books.Count;
            else
                skip = 0;
            previousNewerThan = newerThan;
            newerThan = books.Max(x => x.TimeAdded);
            latestDate = MaxDate(latestDate ?? DateTime.MinValue, books.Max(x => x.LatestChange));
            bookCounter += books.Count;
            books = await _bookRepository.GetLibgenForIndex(take, newerThan, skip);
        }
        if (latestDate is not null)
        {
            _logger.LogInformation(
                "Updated {BooksCounter} books for {BookSource}",
                bookCounter,
                "LibgenFiction"
            );
            metadata.LastestLibgenEntry = latestDate.Value;
            await _metadataRepository.UpdateMetadata(metadata);
        }
    }

    public async Task IndexLatestLibgenFiction(CancellationToken cancellationToken)
    {
        var take = 10000;
        var metadata = await _metadataRepository.GetMetadata();
        var newerThan = metadata.LastestLibgenFictionEntry;
        var previousNewerThan = (DateTime?)null;
        var skip = 0;
        var latestDate = (DateTime?)null;
        var bookCounter = 0;

        var books = await _bookRepository.GetLibgenFictionForIndex(take, newerThan, skip);
        while (books.Any())
        {
            var mappedBooks = books.Select(x => Map(x, "LibgenFiction")).ToList();
            var response = await _elasticClient.IndexManyAsync(
                mappedBooks,
                cancellationToken: cancellationToken
            );
            if (previousNewerThan == newerThan)
                skip += books.Count;
            else
                skip = 0;
            previousNewerThan = newerThan;
            newerThan = books.Max(x => x.TimeAdded);
            latestDate = MaxDate(latestDate ?? DateTime.MinValue, books.Max(x => x.LatestChange));
            bookCounter += books.Count;
            books = await _bookRepository.GetLibgenFictionForIndex(take, newerThan, skip);
        }

        if (latestDate is not null)
        {
            _logger.LogInformation(
                "Updated {BooksCounter} books for {BookSource}",
                bookCounter,
                "LibgenFiction"
            );
            metadata.LastestLibgenFictionEntry = latestDate.Value;
            await _metadataRepository.UpdateMetadata(metadata);
        }
    }

    public async Task IndexZlibrary()
    {
        var take = 10000;
        uint skip = 0;

        var books = await _bookRepository.GetZlibForIndex(take, skip);
        while (books.Any())
        {
            var mappedBooks = books.Select(x => Map(x, "ZLibrary")).ToList();
            var response = await _elasticClient.IndexManyAsync(mappedBooks);
            skip = books.Max(x => x.Id);
            books = await _bookRepository.GetZlibForIndex(take, skip);
        }
    }

    private static DateTime MaxDate(params DateTime[] dates) => dates.Max();

    private static ElasticBook Map(Book x, string source) =>
        new()
        {
            Author = x.Author,
            Extension = x.Extension,
            Filesize = x.Filesize,
            Id = x.IpfsCid,
            Identifier = x.Identifier,
            IpfsCid = x.IpfsCid,
            Language = x.Language,
            LatestChange = x.LatestChange,
            Locator = x.Locator,
            Md5 = x.Md5,
            Pages = x.Pages,
            Publisher = x.Publisher,
            Title = x.Title,
            Year = x.Year,
            Source = source,
        };
}
