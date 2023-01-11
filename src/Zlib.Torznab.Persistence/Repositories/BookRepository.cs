using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Zlib.Torznab.Models.Archive;
using Zlib.Torznab.Models.Repositories;
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

    public async Task<IReadOnlyList<Book>> GetAllLibgenFictionForIndex(int limit, uint largerThanId)
    {
        _context.Database.SetCommandTimeout(60);
        return await GetFictionQuery()
            .Where(
                x =>
                    x.Id > largerThanId
                    && x.IpfsCid != ""
#pragma warning disable MA0002 // Does not work with LINQ to entities
                    && AllowedExtensions.Contains(x.Extension)
#pragma warning restore MA0002
                    && (x.Language == "" || x.Language == "English" || x.Language == "other")
            )
            .OrderBy(x => x.Id)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Book>> GetLatestLibgenFictionForIndex(int limit, int skip)
    {
        _context.Database.SetCommandTimeout(60);
        return await GetFictionQuery()
            .Where(
                x =>
                    x.IpfsCid != ""
#pragma warning disable MA0002 // Does not work with LINQ to entities
                    && AllowedExtensions.Contains(x.Extension)
#pragma warning restore MA0002
                    && (x.Language == "" || x.Language == "English" || x.Language == "other")
            )
            .OrderByDescending(x => x.TimeModified)
            .Skip(skip)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Book>> GetAllLibgenForIndex(int limit, uint largerThanId)
    {
        _context.Database.SetCommandTimeout(60);
        return await GetLibgenQuery()
            .Where(
                x =>
                    x.Id > largerThanId
                    && x.IpfsCid != ""
#pragma warning disable MA0002 // Does not work with LINQ to entities
                    && AllowedExtensions.Contains(x.Extension)
#pragma warning restore MA0002
                    && (x.Language == "" || x.Language == "English" || x.Language == "other")
            )
            .OrderBy(x => x.Id)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Book>> GetLatestLibgenForIndex(int limit, int skip)
    {
        _context.Database.SetCommandTimeout(60);
        return await GetLibgenQuery()
            .Where(
                x =>
                    x.IpfsCid != ""
#pragma warning disable MA0002 // Does not work with LINQ to entities
                    && AllowedExtensions.Contains(x.Extension)
#pragma warning restore MA0002
                    && (x.Language == "" || x.Language == "English" || x.Language == "other")
            )
            .OrderByDescending(x => x.TimeModified)
            .Skip(skip)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Book>> GetZlibForIndex(int limit, uint offset)
    {
        _context.Database.SetCommandTimeout(120);
        return await GetZlibQuery(
                x =>
#pragma warning disable MA0002 // Does not work with LINQ to entities
                    AllowedExtensions.Contains(x.Extension)
#pragma warning restore MA0002
                    && (x.Language == "" || x.Language == "English" || x.Language == "other")
                    && x.Id > offset
            )
            .OrderBy(x => x.Id)
            // .Skip(offset)
            .Take(limit)
            .ToListAsync();
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
        var query = _context.ZlibBooks
            .Join(_context.ZlibIpfs, x => x.Id, x => x.Id, (book, ipfs) => book)
            .AsQueryable();
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
