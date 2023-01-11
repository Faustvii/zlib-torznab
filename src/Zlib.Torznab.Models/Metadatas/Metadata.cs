namespace Zlib.Torznab.Models.Metadatas;

public class Metadata
{
    public int Id { get; set; }
    public DateTime LatestUpdate { get; set; }
    public DateTime LastLibgenImport { get; set; }
    public DateTime LastFictionImport { get; set; }
    public DateTime LastestLibgenEntry { get; set; }
    public DateTime LastestLibgenFictionEntry { get; set; }
    public uint LatestLibgenFictionEntryId { get; set; }
    public uint LatestLibgenEntryId { get; set; }
}
