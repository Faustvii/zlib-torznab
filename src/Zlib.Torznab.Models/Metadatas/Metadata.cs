namespace Zlib.Torznab.Models.Metadatas;

public class Metadata
{
    public int Id { get; set; }
    public DateTime LatestUpdate { get; set; }
    public DateTime LastLibgenImport { get; set; }
    public DateTime LastFictionImport { get; set; }
}
