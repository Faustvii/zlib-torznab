namespace Zlib.Torznab.Models.Settings;

public class TorrentSettings
{
    public const string Key = "Settings:Torrent";

    public string TrackerUrl { get; set; } = string.Empty;
    public string DownloadDirectory { get; set; } = string.Empty;
    public string NetworkInterface { get; set; } = string.Empty;
    public int Port { get; set; }
    public Dictionary<string, string> TranslateIps { get; set; } = new Dictionary<string, string>();
}
