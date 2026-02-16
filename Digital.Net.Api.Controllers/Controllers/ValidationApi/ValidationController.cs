using Digital.Net.Api.Authentication.Attributes;
using Digital.Net.Api.Core.Extensions.StringUtilities;
using Digital.Net.Api.Core.Settings;
using Microsoft.AspNetCore.Mvc;

namespace Digital.Net.Api.Controllers.Controllers.ValidationApi;

[ApiController, Route("validation")]
public class ValidationController : ControllerBase
{
    [HttpGet("pattern/email")]
    public ActionResult<string> GetEmailPattern() => Ok(RegularExpressions.EmailPattern);

    [HttpGet("pattern/username"), Authorize(AuthorizeType.Any)]
    public ActionResult<string> GetUsernamePattern() => Ok(RegularExpressions.UsernamePattern);

    [HttpGet("pattern/password"), Authorize(AuthorizeType.Any)]
    public ActionResult<string> GetPasswordPattern() => Ok(RegularExpressions.PasswordPattern);

    [HttpGet("size/avatar"), Authorize(AuthorizeType.Any)]
    public ActionResult<long> GetAvatarMaxSize() => Ok(AppSettings.DefaultMaxAvatarSize);
}