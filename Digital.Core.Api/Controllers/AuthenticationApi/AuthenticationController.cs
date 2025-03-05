using Digital.Core.Api.Controllers.AuthenticationApi.Dto;
using Digital.Lib.Net.Authentication.Attributes;
using Digital.Lib.Net.Authentication.Exceptions;
using Digital.Lib.Net.Core.Messages;
using Digital.Lib.Net.Authentication.Services.Authentication;
using Digital.Lib.Net.Core.Extensions.ExceptionUtilities;
using Microsoft.AspNetCore.Mvc;

namespace Digital.Core.Api.Controllers.AuthenticationApi;

[ApiController, Route("authentication/user")]
public class AuthenticationController(
    IAuthenticationService authenticationService
) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<Result<string>>> Login([FromBody] LoginPayload request)
    {
        var result = await authenticationService.LoginAsync(request.Login, request.Password);

        if (result.Errors.Any(e => e.Reference == new TooManyAttemptsException().GetReference()))
            return StatusCode(429);
        if (result.HasError() || result.Value is null)
            return Unauthorized();
        return Ok(result);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<Result<string>>> RefreshTokens()
    {
        var result = await authenticationService.RefreshTokensAsync();
        return result.HasError() ? Unauthorized() : Ok(result);
    }

    [HttpPost("logout"), Authorize(AuthorizeType.Jwt)]
    public async Task<IActionResult> Logout()
    {
        await authenticationService.LogoutAsync();
        return NoContent();
    }

    [HttpPost("logout-all"), Authorize(AuthorizeType.Any)]
    public async Task<IActionResult> LogoutAll()
   {
        await authenticationService.LogoutAllAsync();
        return NoContent();
    }
}