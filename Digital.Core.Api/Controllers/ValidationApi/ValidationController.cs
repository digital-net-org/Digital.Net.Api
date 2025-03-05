using Digital.Lib.Net.Authentication.Attributes;
using Digital.Lib.Net.Authentication.Options;
using Digital.Lib.Net.Core.Extensions.StringUtilities;
using Digital.Lib.Net.Files.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Digital.Core.Api.Controllers.ValidationApi;

[ApiController, Route("validation")]
public class ValidationController(
    IOptions<DigitalFilesOptions> filesOptions,
    IOptions<AuthenticationOptions> authenticationOptions
) : ControllerBase
{
    [HttpGet("email/pattern")]
    public ActionResult<string> GetEmailPattern() => Ok(RegularExpressions.EmailPattern);

    [HttpGet("username/pattern"), Authorize(AuthorizeType.Any)]
    public ActionResult<string> GetUsernamePattern() => Ok(RegularExpressions.UsernamePattern);

    [HttpGet("password/pattern"), Authorize(AuthorizeType.Any)]
    public ActionResult<string> GetPasswordPattern() => Ok(authenticationOptions.Value.PasswordConfig.PasswordRegex);

    [HttpGet("avatar/size"), Authorize(AuthorizeType.Any)]
    public ActionResult<long> GetAvatarMaxSize() => Ok(filesOptions.Value.MaxAvatarSize);
}