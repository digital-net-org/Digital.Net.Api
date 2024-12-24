using Digital.Net.Core.Application;
using Microsoft.AspNetCore.Mvc;

namespace SafariDigital.Api.Controllers;

[ApiController]
public class PublicController : ControllerBase
{
    [HttpGet("/ping")]
    public ActionResult<string> Ping() => Ok("Pong!");

    [HttpGet("/version")]
    public ActionResult<string> Version() => Ok(ApplicationVersion.GetAssemblyVersion());
}