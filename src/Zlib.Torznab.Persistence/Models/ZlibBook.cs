using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zlib.Torznab.Persistence.Models;

public class ZlibBook
{
    [Key]
    [Column("zlibrary_id")]
    public uint Id { get; set; }

    [Column("date_added")]
    [DataType("timestamp")]
    public DateTime Added { get; set; }

    [Column("date_modified")]
    [DataType("timestamp")]
    public DateTime Modified { get; set; }
    public string Extension { get; set; } = string.Empty;
    public uint? FileSize { get; set; }

    [Column("filesize_reported")]
    public uint FileSizeReported { get; set; }
    public string? Md5 { get; set; }

    [Column("md5_reported")]
    public string Md5Reported { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Publisher { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public string Year { get; set; } = string.Empty;
    public string Series { get; set; } = string.Empty;
    public string Pages { get; set; } = string.Empty;

    [ForeignKey(nameof(Id))]
    public ZlibIpfs Ipfs { get; set; } = new();
}
