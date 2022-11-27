namespace Zlib.Torznab.Models.Settings;

public class TorznabSettings
{
    public const string Key = "Settings:Torznab";

    public string SourceUrlBase { get; set; } = string.Empty;
    public string TorrentDownloadBase { get; set; } = string.Empty;
}
