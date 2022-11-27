namespace Zlib.Torznab.Models.Settings;

public class IpfsSettings
{
    public const string Key = "Settings:IPFS";

    public string Gateway { get; set; } = string.Empty;
}
