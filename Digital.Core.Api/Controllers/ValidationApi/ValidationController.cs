using Digital.Lib.Net.Authentication.Attributes;
using Digital.Lib.Net.Core.Extensions.StringUtilities;
using Microsoft.AspNetCore.Mvc;

namespace Digital.Core.Api.Controllers.ValidationApi;

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
    public ActionResult<long> GetAvatarMaxSize() => Ok(ApplicationDefaults.MaxAvatarSize);
}