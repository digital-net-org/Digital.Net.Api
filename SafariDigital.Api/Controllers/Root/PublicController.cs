using Microsoft.AspNetCore.Mvc;
using SafariDigital.Core.Application;

namespace SafariDigital.Api.Controllers.Root;

[ApiController]
public class PublicController : ControllerBase
{
    [HttpGet("/ping")]
    public IActionResult Ping() => Ok("Pong!");

    [HttpGet("/version")]
    public IActionResult Version() => Ok(ApplicationVersion.GetAssemblyVersion());
}