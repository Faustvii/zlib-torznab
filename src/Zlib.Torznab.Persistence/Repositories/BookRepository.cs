using System.Diagnostics.CodeAnalysis;
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
        var fiction = await GetFictionQueryable().FirstOrDefaultAsync(x => x.Fiction.Md5 == ipfs);
        var libgen = await GetLibgenQueryable().FirstOrDefaultAsync(x => x.Libgen.Md5 == ipfs);

        return MapItem(fiction) ?? MapItem(libgen);
    }

    private IQueryable<FictionRecord> GetFictionQueryable()
    {
        return _context.Fictions
            .Join(
                _context.FictionHashes,
                f => f.Md5,
                fh => fh.Md5,
                (f, fh) => new FictionRecord { Fiction = f, Hash = fh }
            )
            .Where(
                x =>
                    x.Hash.IpfsCid != null
                    && x.Hash.IpfsCid != ""
#pragma warning disable MA0002 // Does not work with LINQ to entities
                    && AllowedExtensions.Contains(x.Fiction.Extension)
#pragma warning restore MA0002
            );
    }

    private IQueryable<LibgenRecord> GetLibgenQueryable()
    {
        return _context.Libgen
            .Join(
                _context.LibgenHashes,
                f => f.Md5,
                fh => fh.Md5,
                (f, fh) => new LibgenRecord { Libgen = f, Hash = fh }
            )
            .Where(
                x =>
                    x.Hash.IpfsCid != null
                    && x.Hash.IpfsCid != ""
#pragma warning disable MA0002 // Does not work with LINQ to entities
                    && AllowedExtensions.Contains(x.Libgen.Extension)
#pragma warning restore MA0002
            );
    }

    public async Task<IReadOnlyList<Book>> GetBooksFromTorznabQuery(TorznabRequest request)
    {
        var fictionQuery = ApplyFiltering(GetFictionQuery(), request);
        var libgenQuery = ApplyFiltering(GetLibgenQuery(), request);

        return await fictionQuery
            .OrderByDescending(x => x.Id)
            .Take(request.Limit)
            .Skip(request.Offset)
            .Concat(
                libgenQuery.OrderByDescending(x => x.Id).Skip(request.Offset).Take(request.Limit)
            )
            .OrderByDescending(x => x.TimeAdded)
            .Skip(request.Offset)
            .Take(request.Limit)
            .ToListAsync();
    }

    private static IQueryable<Book> ApplyFiltering(IQueryable<Book> query, TorznabRequest request)
    {
        query = query.Where(
            x =>
                x.IpfsCid != ""
#pragma warning disable MA0002 // Does not work with LINQ to entities
                && AllowedExtensions.Contains(x.Extension)
                && (x.Language == "" || x.Language == "English")
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
            .Where(
                x =>
                    x.Hash.IpfsCid != ""
#pragma warning disable MA0002 // Does not work with LINQ to entities
                    && AllowedExtensions.Contains(x.Libgen.Extension)
#pragma warning restore MA0002
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
                        IpfsCid = x.Hash.IpfsCid,
                        Filesize = x.Libgen.Filesize,
                        Pages = x.Libgen.Pages,
                        Language = x.Libgen.Language,
                        Publisher = x.Libgen.Publisher,
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
            //             .Where(
            //                 x =>
            //                     x.Hash.IpfsCid != ""
            // #pragma warning disable MA0002 // Does not work with LINQ to entities
            //                     && AllowedExtensions.Contains(x.Fiction.Extension)
            // #pragma warning restore MA0002
            //             )
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
                        IpfsCid = x.Hash.IpfsCid,
                        Filesize = x.Fiction.Filesize,
                        Pages = x.Fiction.Pages,
                        Language = x.Fiction.Language,
                        Publisher = x.Fiction.Publisher,
                    }
            );
        return fictionQuery;
    }

    [return: NotNullIfNotNull(nameof(entity))]
    private static Book? MapItem(FictionRecord? entity)
    {
        if (entity is null)
            return null;

        return new Book
        {
            Id = entity.Fiction.Id,
            Md5 = entity.Fiction.Md5,
            Author = entity.Fiction.Author,
            Title = entity.Fiction.Title,
            Year = entity.Fiction.Year,
            Extension = entity.Fiction.Extension,
            TimeAdded = entity.Fiction.TimeAdded,
            IpfsCid = entity.Hash.IpfsCid,
            Filesize = entity.Fiction.Filesize,
            Pages = entity.Fiction.Pages,
            Language = entity.Fiction.Language,
            Publisher = entity.Fiction.Publisher,
        };
    }

    [return: NotNullIfNotNull(nameof(entity))]
    private static Book? MapItem(LibgenRecord? entity)
    {
        if (entity is null)
            return null;

        return new Book
        {
            Id = entity.Libgen.Id,
            Md5 = entity.Libgen.Md5,
            Author = entity.Libgen.Author,
            Title = entity.Libgen.Title,
            Year = entity.Libgen.Year,
            Extension = entity.Libgen.Extension,
            TimeAdded = entity.Libgen.TimeAdded,
            IpfsCid = entity.Hash.IpfsCid,
            Filesize = entity.Libgen.Filesize,
            Pages = entity.Libgen.Pages,
            Language = entity.Libgen.Language,
            Publisher = entity.Libgen.Publisher,
        };
    }
}
