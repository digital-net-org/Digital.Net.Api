using Digital.Pages.Api.Attributes;
using Digital.Lib.Net.Authentication.Attributes;
using Microsoft.AspNetCore.Mvc;
using Digital.Pages.Core;
using Digital.Pages.Core.Application;

namespace Digital.Pages.Api.Controllers;

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