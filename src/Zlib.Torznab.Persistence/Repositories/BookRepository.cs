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

    public async Task<Book?> GetBookFromMd5(string ipfs)
    {
        var book = await GetFictionQuery()
            .Concat(GetLibgenQuery())
            .FirstOrDefaultAsync(x => x.Md5 == ipfs);
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

    private IQueryable<Book> GetLibgenQuery()
    {
        var libgenQuery = _context.Libgen
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
                        TimeAdded = x.Libgen.TimeLastModified ?? x.Libgen.TimeAdded,
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

    private IQueryable<Book> GetFictionQuery()
    {
        var fictionQuery = _context.Fictions
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
                        TimeAdded = x.Fiction.TimeLastModified ?? x.Fiction.TimeAdded,
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

    private IQueryable<Book> GetZlibQuery()
    {
        var query = _context.ZlibBooks.Select(
            x =>
                new Book
                {
                    Id = x.Id,
                    Md5 = x.Md5 ?? x.Md5Reported,
                    Author = x.Author,
                    Title = x.Title,
                    Year = x.Year,
                    Extension = x.Extension,
                    TimeAdded = x.Modified,
                    IpfsCid = x.Ipfs.IpfsCid,
                    Filesize = x.FileSize ?? x.FileSizeReported,
                    Pages = x.Pages,
                    Language = x.Language,
                    Publisher = x.Publisher,
                    Locator = x.Md5 ?? x.Md5Reported,
                    Identifier = string.Empty,
                }
        );
        return query;
    }
}
