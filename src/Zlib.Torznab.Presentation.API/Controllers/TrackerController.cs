using System.Net;
using Microsoft.AspNetCore.Mvc;
using Zlib.Torznab.Presentation.API.Services;

namespace Zlib.Torznab.Presentation.API.Controllers;

[ApiController]
[Route("[Controller]/[action]")]
public class TrackerController : ControllerBase
{
    private readonly APITrackerListener _trackerListener;

    public TrackerController(APITrackerListener trackerListener)
    {
        _trackerListener = trackerListener;
    }

    [HttpGet]
    [ProducesResponseType(typeof(FileContentResult), 200)]
#pragma warning disable IDE0060
    public IActionResult Announce(
        string info_hash,
        string peer_id,
        string port,
        string uploaded,
        string downloaded,
        string left,
        string compact,
        string numwant,
        string supportcrypto,
        string key,
        string @event
    )
#pragma warning restore IDE0060
    {
        //"/announce/?info_hash=%de~%8c%a6ov%5d%88%88g%bc%fc%05J%ec%de7%e5%fc%07&peer_id=-MO3000-786463501433&port=37647&uploaded=0&downloaded=0&left=0&compact=1&numwant=100&supportcrypto=1&key=MO3000-1459076085&event=started"
        var ip = ExtractProxyAwareIp();
        if (ip is null || Request.QueryString.Value is null)
            return BadRequest();

        var response = _trackerListener.Handle(Request.QueryString.Value, ip, isScrape: false);
        // var responseString = Encoding.UTF8.GetString(response.Encode());
        return File(response.Encode(), "text/plain");
        // return Content(responseString);
    }

    [HttpGet]
    [ProducesResponseType(typeof(FileContentResult), 200)]
#pragma warning disable IDE0060
    public IActionResult Scrape([FromQuery(Name = "info_hash")] string[] hashes)
#pragma warning restore IDE0060
    {
        var ip = ExtractProxyAwareIp();
        if (ip is null || Request.QueryString.Value is null)
            return BadRequest();

        var response = _trackerListener.Handle(Request.QueryString.Value, ip, isScrape: true);

        //GET:/scrape/?info_hash=%de~%8c%a6ov%5d%88%88g%bc%fc%05J%ec%de7%e5%fc%07&info_hash=%ed%ff%12%c4J%5EM%9e%1c%3a%fd%2f!%0c%89%aci%CA%BE%f4
        return File(response.Encode(), "text/plain");
    }

    private IPAddress? ExtractProxyAwareIp()
    {
        var ip = Request.HttpContext.Connection.RemoteIpAddress;
        var forwardedForIp = Request.Headers["x-forwarded-for"].FirstOrDefault();
        if (forwardedForIp is not null)
            ip = IPAddress.Parse(forwardedForIp);
        return ip?.MapToIPv4();
    }
}
