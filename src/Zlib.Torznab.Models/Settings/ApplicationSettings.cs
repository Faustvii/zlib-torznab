namespace Zlib.Torznab.Models.Settings;

public class ApplicationSettings
{
    public const string Key = "Settings";

    public IpfsSettings Ipfs { get; set; } = new();
    public TorrentSettings Torrent { get; set; } = new();
    public TorznabSettings Torznab { get; set; } = new();
    public MetadataSettings Metadata { get; set; } = new();
    public ElasticSettings Elastic { get; set; } = new();
}
