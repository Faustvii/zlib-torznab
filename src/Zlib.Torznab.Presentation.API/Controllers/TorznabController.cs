using Microsoft.AspNetCore.Mvc;
using Zlib.Torznab.Models.Torznab;
using Zlib.Torznab.Presentation.API.Dtos;
using Zlib.Torznab.Services.Torznab;

namespace Zlib.Torznab.Presentation.API.Controllers;

[ApiController]
[Route("api")]
public class TorznabController : ControllerBase
{
    private readonly ITorznabService _torznabService;

    public TorznabController(ITorznabService torznabService)
    {
        _torznabService = torznabService;
    }

    [HttpGet()]
    [Produces("application/xml")]
    public async Task<IActionResult> Torznab([FromQuery] TorznabInputDto request)
    {
        TorznabResponseBase response = request.Type.ToLowerInvariant() switch
        {
            "caps" => await _torznabService.GetCapabilities(),
            _
                => await _torznabService.GetFeed(
                    new TorznabRequest(
                        request.Categories,
                        request.Query,
                        request.Author,
                        request.Title,
                        request.Year,
                        request.Limit,
                        request.Offset
                    )
                ),
        };

        return Ok(response);
    }
}
