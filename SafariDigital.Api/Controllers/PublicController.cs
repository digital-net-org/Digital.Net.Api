using Microsoft.AspNetCore.Mvc;

namespace SafariDigital.Api.Controllers;

[ApiController]
public class PublicController : ControllerBase
{
    [HttpGet("/ping")]
    public IActionResult Ping() => Ok("Pong!");
}