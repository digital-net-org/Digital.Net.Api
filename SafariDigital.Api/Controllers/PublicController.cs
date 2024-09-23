using Microsoft.AspNetCore.Mvc;
using SafariDigital.Core.Application;

namespace SafariDigital.Api.Controllers;

[ApiController]
public class PublicController : ControllerBase
{
    [HttpGet("/ping")]
    public ActionResult<string> Ping() => Ok("Pong!");

    [HttpGet("/version")]
    public ActionResult<string> Version() => Ok(ApplicationVersion.GetAssemblyVersion());
}