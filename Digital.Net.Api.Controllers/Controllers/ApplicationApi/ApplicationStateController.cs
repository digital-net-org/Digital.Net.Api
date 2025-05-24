using Digital.Net.Api.Services.Application;
using Microsoft.AspNetCore.Mvc;

namespace Digital.Net.Api.Controllers.Controllers.ApplicationApi;

[ApiController]
public class ApplicationStateController(
    IApplicationService applicationService
) : ControllerBase
{
    [HttpGet("/")]
    public ActionResult<ApplicationVersion> GetVersion() => Ok(applicationService.GetVersion());
}