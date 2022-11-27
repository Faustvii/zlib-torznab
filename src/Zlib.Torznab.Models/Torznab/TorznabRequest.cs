namespace Zlib.Torznab.Models.Torznab;

public record TorznabRequest(
    string[] Categories,
    int Limit,
    int Offset,
    string? Query,
    string? Author,
    string? Title,
    string? Year
);
