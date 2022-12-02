namespace Zlib.Torznab.Services.Metadata;

public interface IMetadataService
{
    Task ImportLatestData(CancellationToken cancellationToken);
}
