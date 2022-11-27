namespace Zlib.Torznab.Persistence.Models;

internal class FictionRecord
{
    public FictionRecord() { }

    public Fiction Fiction { get; set; } = null!;
    public FictionHash Hash { get; set; } = null!;
}
