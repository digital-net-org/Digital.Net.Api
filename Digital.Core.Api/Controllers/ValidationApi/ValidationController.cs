using Digital.Lib.Net.Authentication.Attributes;
using Digital.Lib.Net.Core.Extensions.StringUtilities;
using Microsoft.AspNetCore.Mvc;

namespace Digital.Core.Api.Controllers.ValidationApi;

[ApiController, Route("validation")]
public class ValidationController : ControllerBase
{
    [HttpGet("email/pattern")]
    public ActionResult<string> GetEmailPattern() => Ok(RegularExpressions.EmailPattern);

    [HttpGet("username/pattern"), Authorize(AuthorizeType.Any)]
    public ActionResult<string> GetUsernamePattern() => Ok(RegularExpressions.UsernamePattern);

    [HttpGet("password/pattern"), Authorize(AuthorizeType.Any)]
    public ActionResult<string> GetPasswordPattern() => Ok(RegularExpressions.PasswordPattern);

    [HttpGet("avatar/size"), Authorize(AuthorizeType.Any)]
    public ActionResult<long> GetAvatarMaxSize() => Ok(ApplicationDefaults.MaxAvatarSize);
}