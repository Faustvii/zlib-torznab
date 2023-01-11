using Microsoft.AspNetCore.Mvc;
using Zlib.Torznab.Models.Archive;
using Zlib.Torznab.Services.Elastic;

namespace Zlib.Torznab.Presentation.API.Controllers;

[ApiController]
[Route("[controller]")]
public class ElasticController : ControllerBase
{
    private readonly IElasticService _elasticService;

    public ElasticController(IElasticService elasticService)
    {
        _elasticService = elasticService;
    }

    [HttpGet]
    [Route("{ipfsCid}")]
    [ProducesResponseType(typeof(Book), 200)]
    public async Task<IActionResult> GetBook(string ipfsCid)
    {
        var book = await _elasticService.GetBookByIPFS(ipfsCid);
        return Ok(book);
    }

    [HttpGet]
    [Route("[action]")]
    [ProducesResponseType(typeof(Book), 200)]
    public async Task<IActionResult> IndexAllLibgenFiction(CancellationToken cancellationToken)
    {
        await _elasticService.IndexAllLibgenFiction(cancellationToken);
        return Ok();
    }

    [HttpGet]
    [Route("[action]")]
    [ProducesResponseType(typeof(Book), 200)]
    public async Task<IActionResult> IndexAllLibgen(CancellationToken cancellationToken)
    {
        await _elasticService.IndexAllLibgen(cancellationToken);
        return Ok();
    }
}
