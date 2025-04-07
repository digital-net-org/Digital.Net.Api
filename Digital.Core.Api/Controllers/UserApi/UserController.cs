using System.Text.Json;
using Digital.Core.Api.Controllers.UserApi.Dto;
using Digital.Core.Api.Services.Users;
using Digital.Lib.Net.Authentication.Attributes;
using Digital.Lib.Net.Authentication.Exceptions;
using Digital.Lib.Net.Authentication.Services.Authentication;
using Digital.Lib.Net.Core.Messages;
using Digital.Lib.Net.Entities.Context;
using Digital.Lib.Net.Entities.Models.Documents;
using Digital.Lib.Net.Entities.Models.Users;
using Digital.Lib.Net.Entities.Services;
using Digital.Lib.Net.Mvc.Controllers.Crud;
using Microsoft.AspNetCore.Mvc;

namespace Digital.Core.Api.Controllers.UserApi;

[ApiController, Route("user"), Authorize(AuthorizeType.Jwt)]
public class UserController(
    IEntityService<User, DigitalContext> entityService,
    IAuthenticationService authenticationService,
    IUserService userService
) : CrudController<User, DigitalContext, UserDto, UserDto>(entityService)
{
    [HttpGet("self")]
    public async Task<ActionResult<Result<UserDto>>> GetSelfAsync() =>
        await authenticationService.GetAuthenticatedUserAsync() is { } user
            ? Ok(new Result<UserDto>(new UserDto(user)))
            : Unauthorized();

    [HttpPatch("{id}")]
    public override async Task<ActionResult<Result>> Patch(string id, [FromBody] JsonElement patch)
    {
        var user = await GetAuthorizedUser(Guid.Parse(id));
        if (user is null)
            return Unauthorized();

        return await base.Patch(id, patch);
    }

    [NonAction]
    public override async Task<ActionResult<Result>> Post([FromBody] UserDto payload) => NotFound();

    [NonAction]
    public override async Task<ActionResult<Result>> Delete(string id) => NotFound();

    [HttpPut("{id:guid}/password")]
    public async Task<ActionResult<Result>> UpdatePassword([FromBody] UserPasswordUpdatePayload request, Guid id)
    {
        var user = await GetAuthorizedUser(id);
        if (user is null)
            return Unauthorized();

        var result = await userService.UpdatePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (result.HasError<PasswordMalformedException>())
            return BadRequest(result);
        if (result.HasError<InvalidCredentialsException>())
            return Unauthorized(result);

        return Ok(result);
    }

    [HttpPut("{id:guid}/avatar")]
    public async Task<ActionResult<Result<Document>>> UpdateAvatar(Guid id, IFormFile avatar)
    {
        var user = await GetAuthorizedUser(id);
        if (user is null)
            return Unauthorized();
        return await userService.UpdateAvatar(user, avatar);
    }

    [HttpDelete("{id:guid}/avatar")]
    public async Task<ActionResult<Result>> RemoveAvatar(Guid id)
    {
        var user = await GetAuthorizedUser(id);
        if (user is null)
            return Unauthorized();
        return await userService.RemoveUserAvatar(user);
    }

    private async Task<User?> GetAuthorizedUser(Guid id)
    {
        var user = await authenticationService.GetAuthenticatedUserAsync();
        return user?.Id != id ? null : user;
    }
}