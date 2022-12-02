using System.ComponentModel.DataAnnotations.Schema;

namespace Zlib.Torznab.Persistence.Models;

public class ZlibIpfs
{
    [Column("zlibrary_id")]
    public uint Id { get; set; }

    [Column("ipfs_cid")]
    public string IpfsCid { get; set; } = string.Empty;
}
