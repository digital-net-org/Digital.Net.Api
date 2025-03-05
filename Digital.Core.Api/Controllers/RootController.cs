using Digital.Lib.Net.Authentication.Attributes;
using Digital.Lib.Net.Core.Messages;
using Microsoft.AspNetCore.Mvc;

namespace Digital.Core.Api.Controllers;


[ApiController]
public class RootController(
) : ControllerBase
{
    [HttpGet("/")]
    public ActionResult<Result<string>> GetStatus() => Ok(new Result<string>("Digital Core API"));

    [HttpGet("/version"), Authorize(AuthorizeType.Any)]
    public ActionResult<Result<string>> GetVersion() => Ok(new Result<string>("0.0.0"));
}