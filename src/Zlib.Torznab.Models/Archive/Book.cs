namespace Zlib.Torznab.Models.Archive;

public record Book
{
    public uint Id { get; init; }
    public string Md5 { get; init; } = string.Empty;
    public string Author { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Year { get; init; } = string.Empty;
    public string Extension { get; init; } = string.Empty;
    public DateTime TimeAdded { get; init; }
    public string IpfsCid { get; init; } = string.Empty;
    public uint Filesize { get; init; }
    public string Pages { get; init; } = string.Empty;
    public string Language { get; init; } = string.Empty;
    public string Publisher { get; init; } = string.Empty;
    public string Locator { get; init; } = string.Empty;
    public string Identifier { get; init; } = string.Empty;

    public string FormattedTitle =>
        $"{Author} - {Title} - {Year} - ({Publisher})({Identifier}).{Extension}";
}
