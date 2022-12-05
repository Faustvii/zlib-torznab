namespace Zlib.Torznab.Models.Torznab;

public record TorznabRequest(
    string[] Categories,
    string? Query,
    string? Author,
    string? Title,
    string? Year,
    int Limit = 100,
    int Offset = 0
)
{
    public bool IsRSS => Query is null && Author is null && Title is null && Year is null;
};
