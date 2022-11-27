using Microsoft.AspNetCore.Mvc;

namespace Zlib.Torznab.Presentation.API.Dtos;

public class TorznabInputDto
{
    [FromQuery(Name = "t")]
    public string Type { get; set; } = string.Empty;

    [FromQuery(Name = "cat")]
    public string[] Categories { get; set; } = Array.Empty<string>();

    [FromQuery(Name = "limit")]
    public int Limit { get; set; } = 100;

    [FromQuery(Name = "offset")]
    public int Offset { get; set; } = 0;

    [FromQuery(Name = "q")]
    public string? Query { get; set; }

    [FromQuery(Name = "author")]
    public string? Author { get; set; }

    [FromQuery(Name = "title")]
    public string? Title { get; set; }

    [FromQuery(Name = "year")]
    public string? Year { get; set; }

    [FromQuery(Name = "extended")]
    public string? Extended { get; set; }
}
