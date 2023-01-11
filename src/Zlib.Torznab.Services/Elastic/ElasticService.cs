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
                            q.Match(
                                m =>
                                    m.Field(f => f.Author)
                                        .Query(request.Author)
                                        .Operator(Operator.And)
                            )
                            && (
                                q.Match(
                                    m =>
                                        m.Field(f => f.Title)
                                            .Query(request.Title)
                                            .Operator(Operator.And)
                                            .Boost(1000d)
                                )
                                || q.Match(
                                    m =>
                                        m.Field(f => f.Title)
                                            .Query(request.Title)
                                            .MinimumShouldMatch(70d)
                                )
                            )
                    )
                    .Sort(x => x.Descending(SortSpecialField.Score))
                    .Take(request.Limit)
                    .Skip(request.Offset)
        );

        return searchResponse.Documents.Select(Map).ToList();
    }

    public async Task<Book?> GetBookByIPFS(string ipfsCid)
    {
        var response = await _elasticClient.GetAsync<ElasticBook>(ipfsCid);
        if (!response.Found)
            return null;
        return Map(response.Source);
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

    public async Task IndexAllLibgen(CancellationToken cancellationToken)
    {
        var take = 10000;
        var metadata = await _metadataRepository.GetMetadata();
        metadata.LatestLibgenEntryId = 0;
        var books = await _bookRepository.GetAllLibgenForIndex(take, metadata.LatestLibgenEntryId);

        while (books.Any())
        {
            var mappedBooks = books.Select(x => Map(x, "Libgen")).ToList();
            var response = await _elasticClient.IndexManyAsync(
                mappedBooks,
                cancellationToken: cancellationToken
            );
            metadata.LatestLibgenEntryId = books.Max(x => x.Id);
            books = await _bookRepository.GetAllLibgenForIndex(take, metadata.LatestLibgenEntryId);
        }
        await _metadataRepository.UpdateMetadata(metadata);
    }

    public async Task IndexLatestLibgen(CancellationToken cancellationToken)
    {
        var take = 10000;
        var metadata = await _metadataRepository.GetMetadata();
        var latestId = metadata.LatestLibgenEntryId;
        var skip = 0;
        var bookCounter = 0;

        var books = await _bookRepository.GetLatestLibgenForIndex(take, skip);
        var latestBook = books.MaxBy(x => x.LatestChange);
        if (latestBook?.Id == metadata.LatestLibgenEntryId)
        {
            _logger.LogInformation("{BookSource} is up to date", "Libgen");
            return;
        }
        await MapAndIndex(books, "Libgen", cancellationToken);

        while (!books.Any(x => x.Id == metadata.LatestLibgenEntryId))
        {
            skip += books.Count;
            bookCounter += books.Count;
            books = await _bookRepository.GetLatestLibgenForIndex(take, skip);
            await MapAndIndex(books, "Libgen", cancellationToken);
        }

        metadata.LatestLibgenEntryId = latestBook?.Id ?? metadata.LatestLibgenEntryId;
        await _metadataRepository.UpdateMetadata(metadata);

        _logger.LogInformation(
            "Updated {BooksCounter} books for {BookSource}",
            bookCounter,
            "Libgen"
        );
    }

    public async Task IndexAllLibgenFiction(CancellationToken cancellationToken)
    {
        var take = 10000;
        var metadata = await _metadataRepository.GetMetadata();
        metadata.LatestLibgenFictionEntryId = 0;
        var books = await _bookRepository.GetAllLibgenFictionForIndex(
            take,
            metadata.LatestLibgenFictionEntryId
        );

        while (books.Any())
        {
            var mappedBooks = books.Select(x => Map(x, "LibgenFiction")).ToList();
            var response = await _elasticClient.IndexManyAsync(
                mappedBooks,
                cancellationToken: cancellationToken
            );
            metadata.LatestLibgenFictionEntryId = books.Max(x => x.Id);
            books = await _bookRepository.GetAllLibgenFictionForIndex(
                take,
                metadata.LatestLibgenFictionEntryId
            );
        }
        await _metadataRepository.UpdateMetadata(metadata);
    }

    public async Task IndexLatestLibgenFiction(CancellationToken cancellationToken)
    {
        var take = 10000;
        var metadata = await _metadataRepository.GetMetadata();
        var latestId = metadata.LatestLibgenFictionEntryId;
        var skip = 0;
        var bookCounter = 0;

        var books = await _bookRepository.GetLatestLibgenFictionForIndex(take, skip);
        var latestBook = books.MaxBy(x => x.LatestChange);
        if (latestBook?.Id == metadata.LatestLibgenFictionEntryId)
        {
            _logger.LogInformation("{BookSource} is up to date", "LibgenFiction");
            return;
        }
        await MapAndIndex(books, "LibgenFiction", cancellationToken);

        while (!books.Any(x => x.Id == metadata.LatestLibgenFictionEntryId))
        {
            skip += books.Count;
            bookCounter += books.Count;
            books = await _bookRepository.GetLatestLibgenFictionForIndex(take, skip);
            await MapAndIndex(books, "LibgenFiction", cancellationToken);
        }

        metadata.LatestLibgenFictionEntryId = latestBook?.Id ?? metadata.LatestLibgenFictionEntryId;
        await _metadataRepository.UpdateMetadata(metadata);

        _logger.LogInformation(
            "Updated {BooksCounter} books for {BookSource}",
            bookCounter,
            "LibgenFiction"
        );
    }

    private async Task MapAndIndex(
        IReadOnlyList<Book> books,
        string source,
        CancellationToken cancellationToken
    )
    {
        var mappedBooks = books.Select(x => Map(x, source)).ToList();
        var response = await _elasticClient.IndexManyAsync(
            mappedBooks,
            cancellationToken: cancellationToken
        );
    }

    public async Task IndexZlibrary()
    {
        var take = 10000;
        uint skip = 0;

        var books = await _bookRepository.GetZlibForIndex(take, skip);
        while (books.Any())
        {
            await MapAndIndex(books, "ZLibrary", CancellationToken.None);
            skip = books.Max(x => x.Id);
            books = await _bookRepository.GetZlibForIndex(take, skip);
        }
    }

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
