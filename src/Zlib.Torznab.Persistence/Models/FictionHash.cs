namespace Zlib.Torznab.Persistence.Models;

public partial class FictionHash
{
    public string Md5 { get; set; } = null!;

    public string? IpfsCid { get; set; }
}
