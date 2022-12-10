using System.Data.SqlTypes;
using Microsoft.EntityFrameworkCore;
using Zlib.Torznab.Models.Metadatas;
using Zlib.Torznab.Models.Repositories;

namespace Zlib.Torznab.Persistence.Repositories;

public class MetadataRepository : IMetadataRepository
{
    private readonly ArchiveContext _context;

    public MetadataRepository(ArchiveContext context)
    {
        _context = context;
    }

    public async Task<Metadata> GetMetadata()
    {
        var metaData = await _context.Metadata.FirstOrDefaultAsync();
        if (metaData is null)
        {
            metaData = new Models.Metadata
            {
                LastFictionImport = (DateTime)SqlDateTime.MinValue,
                LastLibgenImport = (DateTime)SqlDateTime.MinValue,
                LatestUpdate = (DateTime)SqlDateTime.MinValue,
                LastestLibgenEntry = (DateTime)SqlDateTime.MinValue,
                LastestLibgenFictionEntry = (DateTime)SqlDateTime.MinValue,
            };
            _context.Add(metaData);
            await _context.SaveChangesAsync();
        }
        return new Metadata
        {
            Id = metaData.Id,
            LastFictionImport = metaData.LastFictionImport,
            LastLibgenImport = metaData.LastLibgenImport,
            LatestUpdate = metaData.LatestUpdate,
            LastestLibgenEntry = metaData.LastestLibgenEntry,
            LastestLibgenFictionEntry = metaData.LastestLibgenFictionEntry,
        };
    }

    public async Task UpdateMetadata(Metadata metadata)
    {
        var entity = await _context.Metadata.FirstOrDefaultAsync();
        if (entity is null)
            return;
        entity.LastFictionImport = metadata.LastFictionImport;
        entity.LastLibgenImport = metadata.LastLibgenImport;
        entity.LatestUpdate = metadata.LatestUpdate;
        entity.LastestLibgenEntry = metadata.LastestLibgenEntry;
        entity.LastestLibgenFictionEntry = metadata.LastestLibgenFictionEntry;
        _context.Update(entity);
        await _context.SaveChangesAsync();
    }
}
