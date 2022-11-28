using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Zlib.Torznab.Models.Archive;
using Zlib.Torznab.Models.Repositories;
using Zlib.Torznab.Models.Torznab;
using Zlib.Torznab.Persistence.Models;

namespace Zlib.Torznab.Persistence.Repositories;

public class FictionRepository : IFictionRepository
{
    private readonly ArchiveContext _context;
    private static readonly string[] AllowedExtensions = new[] { "epub", "mobi", "azw3" };

    public FictionRepository(ArchiveContext context)
    {
        _context = context;
    }

    public async Task<Book?> GetFictionFromIpfs(string ipfs)
    {
        var entity = await GetQueryable().FirstOrDefaultAsync(x => x.Hash.IpfsCid == ipfs);
        return MapItem(entity);
    }

    public IQueryable<Book> GetFictionsQueryableFromTorznabQuery(TorznabRequest request)
    {
        var (_, query, author, title, year, limit, offset) = request;
        if (limit > 100)
            limit = 100;
        var queryable = GetQueryable();
        if (!string.IsNullOrWhiteSpace(query))
        {
            queryable = queryable.Where(
                x =>
                    EF.Functions.Match(
                        x.Fiction.Title,
                        request.Query,
                        MySqlMatchSearchMode.NaturalLanguage
                    )
                    && EF.Functions.Match(
                        x.Fiction.Author,
                        request.Query,
                        MySqlMatchSearchMode.NaturalLanguage
                    )
            );
        }

        if (!string.IsNullOrWhiteSpace(author))
            queryable = queryable.Where(
                x =>
                    EF.Functions.Match(
                        x.Fiction.Author,
                        author,
                        MySqlMatchSearchMode.NaturalLanguage
                    )
            );

        if (!string.IsNullOrWhiteSpace(title))
            queryable = queryable.Where(
                x =>
                    EF.Functions.Match(x.Fiction.Title, title, MySqlMatchSearchMode.NaturalLanguage)
            );

        if (!string.IsNullOrWhiteSpace(year))
            queryable = queryable.Where(x => x.Fiction.Year == year);

        return queryable.OrderByDescending(x => x.Fiction.Id).Select(x => MapItem(x));
    }

    public async Task<IReadOnlyList<Book>> GetFictionsFromTorznabQuery(TorznabRequest request)
    {
        return await GetFictionsQueryableFromTorznabQuery(request)
            .Skip(request.Offset)
            .Take(request.Limit)
            .ToListAsync();
    }

    private IQueryable<FictionRecord> GetQueryable()
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

    [return: NotNullIfNotNull(nameof(entity))]
    private static Book? MapItem(FictionRecord? entity)
    {
        if (entity is null)
            return null;

        return new Book(
            entity.Fiction.Id,
            entity.Fiction.Md5,
            entity.Fiction.Author,
            entity.Fiction.Title,
            entity.Fiction.Year,
            entity.Fiction.Extension,
            entity.Fiction.TimeAdded,
            entity.Hash.IpfsCid ?? string.Empty,
            entity.Fiction.Filesize,
            entity.Fiction.Pages,
            entity.Fiction.Language,
            entity.Fiction.Publisher
        );
    }
}
