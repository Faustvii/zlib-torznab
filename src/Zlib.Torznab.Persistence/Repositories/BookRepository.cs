using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Zlib.Torznab.Models.Archive;
using Zlib.Torznab.Models.Repositories;
using Zlib.Torznab.Models.Torznab;
using Zlib.Torznab.Persistence.Models;

namespace Zlib.Torznab.Persistence.Repositories;

public class BookRepository : IBookRepository
{
    private readonly ArchiveContext _context;
    private static readonly string[] AllowedExtensions = new[] { "epub", "mobi", "azw3" };

    public BookRepository(ArchiveContext archiveContext)
    {
        _context = archiveContext;
    }

    public async Task<Book?> GetBookFromMd5(string md5)
    {
        var book = await GetFictionQuery()
            .Concat(GetLibgenQuery())
            .Concat(GetZlibQuery(x => x.Md5 == md5 || x.Md5Reported == md5))
            .FirstOrDefaultAsync(x => x.Md5 == md5);
        return book;
    }

    public async Task<IReadOnlyList<Book>> GetBooksFromTorznabQuery(TorznabRequest request)
    {
        var fictionQuery = ApplyFiltering(GetFictionQuery(), request);
        var libgenQuery = ApplyFiltering(GetLibgenQuery(), request);
        var zlibQuery = ApplyFiltering(GetZlibQuery(), request);

        try
        {
            return await fictionQuery
                .OrderByDescending(x => x.Id)
                .Take(request.Limit)
                .Skip(request.Offset)
                .Concat(
                    libgenQuery
                        .OrderByDescending(x => x.Id)
                        .Skip(request.Offset)
                        .Take(request.Limit)
                )
                .Concat(
                    zlibQuery.OrderByDescending(x => x.Id).Skip(request.Offset).Take(request.Limit)
                )
                .OrderByDescending(x => x.TimeAdded)
                .Skip(request.Offset)
                .Take(request.Limit)
                .ToListAsync();
        }
        catch (System.Exception)
        {
            throw;
        }
    }

    public async Task<IReadOnlyList<Book>> GetBookFeed(DateTime newerThan, int limit, int offset)
    {
        var request = new TorznabRequest(
            Array.Empty<string>(),
            Query: null,
            Author: null,
            Title: null,
            Year: null,
            Limit: limit,
            Offset: offset
        );
        var fictionQuery = ApplyFiltering(GetFictionQuery(orderByDate: true), request)
            .Take(request.Limit * 3);
        var libgenQuery = ApplyFiltering(GetLibgenQuery(orderByDate: true), request)
            .Take(request.Limit * 3);
        var zlibQuery = ApplyFiltering(GetZlibQuery(orderByDate: true), request)
            .Take(request.Limit * 3);

        return await fictionQuery
            .Union(libgenQuery)
            .Union(zlibQuery)
            .Where(x => x.TimeModified < newerThan && x.TimeAdded < newerThan)
            .OrderByDescending(x => x.TimeAdded)
            .Skip(offset)
            .Take(limit)
            .ToListAsync();
    }
    private static IQueryable<Book> ApplyFiltering(IQueryable<Book> query, TorznabRequest request)
    {
        query = query.Where(
            x =>
                x.IpfsCid != ""
#pragma warning disable MA0002 // Does not work with LINQ to entities
                && AllowedExtensions.Contains(x.Extension)
                && (x.Language == "" || x.Language == "English" || x.Language == "other")
#pragma warning restore MA0002
        );
        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            query = query.Where(
                x =>
                    EF.Functions.Match(x.Title, request.Query, MySqlMatchSearchMode.NaturalLanguage)
                    && EF.Functions.Match(
                        x.Author,
                        request.Query,
                        MySqlMatchSearchMode.NaturalLanguage
                    )
            );
        }

        if (!string.IsNullOrWhiteSpace(request.Author))
        {
            var author = string.Join(
                " ",
                request.Author
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => $"+{x.Trim()}")
                    .ToArray()
            );
            query = query.Where(
                x => EF.Functions.Match(x.Author, author, MySqlMatchSearchMode.Boolean)
            );
        }

        if (!string.IsNullOrWhiteSpace(request.Title))
        {
            var title = string.Join(
                " ",
                request.Title
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => $"+{x.Trim()}")
                    .ToArray()
            );
            query = query.Where(
                x => EF.Functions.Match(x.Title, title, MySqlMatchSearchMode.Boolean)
            );
        }

        if (!string.IsNullOrWhiteSpace(request.Year))
        {
            query = query.Where(x => x.Year == request.Year);
        }

        return query;
    }

    private IQueryable<Book> GetLibgenQuery(
        Expression<Func<Libgen, bool>>? predicate = null,
        bool orderByDate = false
    )
    {
        var query = _context.Libgen.AsQueryable();
        if (predicate != null)
            query = query.Where(predicate);

        if (orderByDate)
            query = query
                .OrderByDescending(x => x.TimeLastModified)
                .ThenByDescending(x => x.TimeAdded);
        var libgenQuery = query
            .Join(
                _context.LibgenHashes,
                f => f.Md5,
                fh => fh.Md5,
                (f, fh) => new LibgenRecord { Libgen = f, Hash = fh }
            )
            .Select(
                x =>
                    new Book
                    {
                        Id = x.Libgen.Id,
                        Md5 = x.Libgen.Md5,
                        Author = x.Libgen.Author,
                        Title = x.Libgen.Title,
                        Year = x.Libgen.Year,
                        Extension = x.Libgen.Extension,
                        TimeAdded = x.Libgen.TimeAdded,
                        TimeModified = x.Libgen.TimeLastModified,
                        IpfsCid = x.Hash.IpfsCid,
                        Filesize = x.Libgen.Filesize,
                        Pages = x.Libgen.Pages,
                        Language = x.Libgen.Language,
                        Publisher = x.Libgen.Publisher,
                        Locator = x.Libgen.Locator,
                        Identifier = string.IsNullOrEmpty(x.Libgen.Asin)
                            ? x.Libgen.Identifier
                            : x.Libgen.Asin,
                    }
            );
        return libgenQuery;
    }

    private IQueryable<Book> GetFictionQuery(
        Expression<Func<Fiction, bool>>? predicate = null,
        bool orderByDate = false
    )
    {
        var query = _context.Fictions.AsQueryable();
        if (predicate != null)
            query = query.Where(predicate);

        if (orderByDate)
            query = query
                .OrderByDescending(x => x.TimeLastModified)
                .ThenByDescending(x => x.TimeAdded);

        var fictionQuery = query
            .Join(
                _context.FictionHashes,
                f => f.Md5,
                fh => fh.Md5,
                (f, fh) => new FictionRecord { Fiction = f, Hash = fh }
            )
            .Select(
                x =>
                    new Book
                    {
                        Id = x.Fiction.Id,
                        Md5 = x.Fiction.Md5,
                        Author = x.Fiction.Author,
                        Title = x.Fiction.Title,
                        Year = x.Fiction.Year,
                        Extension = x.Fiction.Extension,
                        TimeAdded = x.Fiction.TimeAdded,
                        TimeModified = x.Fiction.TimeLastModified,
                        IpfsCid = x.Hash.IpfsCid,
                        Filesize = x.Fiction.Filesize,
                        Pages = x.Fiction.Pages,
                        Language = x.Fiction.Language,
                        Publisher = x.Fiction.Publisher,
                        Locator = x.Fiction.Locator,
                        Identifier = string.IsNullOrEmpty(x.Fiction.Asin)
                            ? x.Fiction.Identifier
                            : x.Fiction.Asin,
                    }
            );
        return fictionQuery;
    }

    private IQueryable<Book> GetZlibQuery(
        Expression<Func<ZlibBook, bool>>? predicate = null,
        bool orderByDate = false
    )
    {
        var query = _context.ZlibBooks.AsQueryable();
        if (predicate != null)
            query = query.Where(predicate);
        if (orderByDate)
            query = query.OrderByDescending(x => x.Modified).ThenByDescending(x => x.Added);

        return query.Select(
            x =>
                new Book
                {
                    Id = x.Id,
                    Md5 = x.Md5 ?? x.Md5Reported,
                    Author = x.Author,
                    Title = x.Title,
                    Year = x.Year,
                    Extension = x.Extension,
                    TimeAdded = x.Added,
                    TimeModified = x.Modified,
                    IpfsCid = x.Ipfs.IpfsCid,
                    Filesize = x.FileSize ?? x.FileSizeReported,
                    Pages = x.Pages,
                    Language = x.Language,
                    Publisher = x.Publisher,
                    Locator = x.Md5 ?? x.Md5Reported,
                    Identifier = string.Empty,
                }
        );
    }
}
