namespace Zlib.Torznab.Persistence.Models;

internal class LibgenRecord
{
    public LibgenRecord() { }

    public Libgen Libgen { get; set; } = null!;
    public LibgenHash Hash { get; set; } = null!;
}
