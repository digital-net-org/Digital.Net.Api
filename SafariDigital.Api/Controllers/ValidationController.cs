using Microsoft.AspNetCore.Mvc;
using SafariDigital.Api.Attributes;
using SafariDigital.Core;
using SafariDigital.Core.Application;
using SafariDigital.Data.Models.Database;
using SafariDigital.Services.Authentication;

namespace SafariDigital.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class ValidationController(IConfiguration configuration) : ControllerBase
{
    [HttpGet("username/pattern")]
    public IActionResult GetUsernamePattern() => Ok(RegularExpressions.GetUsernameRegex().ToString());

    [HttpGet("email/pattern")]
    public IActionResult GetEmailPattern() => Ok(RegularExpressions.GetEmailRegex().ToString());

    [Authorize(Role = EUserRole.User)]
    [HttpGet("password/pattern")]
    public IActionResult GetPasswordPattern() => Ok(configuration.GetPasswordRegex().ToString());

    [HttpGet("avatar/size")]
    public IActionResult GetAvatarMaxSize() =>
        Ok(configuration.GetSection<long>(EApplicationSetting.FileSystemMaxAvatarSize));
}