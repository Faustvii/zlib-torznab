namespace Zlib.Torznab.Models.Torznab;

public record TorznabRequest(
    string[] Categories,
    string? Query,
    string? Author,
    string? Title,
    string? Year,
    int Limit = 100,
    int Offset = 0
);
