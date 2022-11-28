namespace Zlib.Torznab.Persistence.Models;

public partial class LibgenHash
{
    public string Md5 { get; set; } = null!;

    public string IpfsCid { get; set; } = string.Empty;
}
