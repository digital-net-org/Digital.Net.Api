using Digital.Net.Authentication.Attributes;
using Microsoft.AspNetCore.Mvc;
using SafariDigital.Api.Attributes;
using SafariDigital.Core;
using SafariDigital.Core.Application;

namespace SafariDigital.Api.Controllers;

[ApiController, Route("validation")]
public class ValidationController(IConfiguration configuration) : ControllerBase
{
    [HttpGet("email/pattern")]
    public ActionResult<string> GetEmailPattern() => Ok(RegularExpressions.EmailPattern);

    [HttpGet("username/pattern"), Authorize(AuthorizeType.Jwt)]
    public ActionResult<string> GetUsernamePattern() => Ok(RegularExpressions.UsernamePattern);

    [HttpGet("password/pattern"), Authorize(AuthorizeType.Jwt)]
    public ActionResult<string> GetPasswordPattern() => Ok(RegularExpressions.PasswordPattern);

    [HttpGet("avatar/size"), Authorize(AuthorizeType.Jwt)]
    public ActionResult<long> GetAvatarMaxSize() =>
        Ok(configuration.GetSection<long>(ApplicationSettingPath.FileSystemMaxAvatarSize));
}