using Zlib.Torznab.Models.Metadatas;

namespace Zlib.Torznab.Models.Repositories;

public interface IMetadataRepository
{
    Task<Metadata> GetMetadata();
    Task UpdateMetadata(Metadata metadata);
}
