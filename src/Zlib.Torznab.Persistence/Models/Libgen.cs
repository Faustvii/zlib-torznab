namespace Zlib.Torznab.Persistence.Models;

public partial class Libgen
{
    public uint Id { get; set; }

    public string Md5 { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string Author { get; set; } = null!;

    public string Series { get; set; } = null!;

    public string Edition { get; set; } = null!;

    public string Language { get; set; } = null!;

    public string Year { get; set; } = null!;

    public string Publisher { get; set; } = null!;

    public string Pages { get; set; } = null!;

    public string Identifier { get; set; } = null!;

    public string GooglebookId { get; set; } = null!;

    public string Asin { get; set; } = null!;

    public string Coverurl { get; set; } = null!;

    public string Extension { get; set; } = null!;

    public uint Filesize { get; set; }

    public string Library { get; set; } = null!;

    public string Locator { get; set; } = null!;

    public string? Commentary { get; set; }

    public string Visible { get; set; } = null!;

    public DateTime TimeAdded { get; set; }

    public DateTime? TimeLastModified { get; set; }
}
