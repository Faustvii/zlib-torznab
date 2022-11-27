namespace Zlib.Torznab.Models.Settings;

public class TorrentSettings
{
    public const string Key = "Settings:Torrent";

    public string TrackerUrl { get; set; } = string.Empty;
    public string DownloadDirectory { get; set; } = string.Empty;
    public string NetworkInterface { get; set; } = string.Empty;
}
