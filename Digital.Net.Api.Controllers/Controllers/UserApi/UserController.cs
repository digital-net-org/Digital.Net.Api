using System.Text.Json;
using Digital.Net.Api.Controllers.Controllers.UserApi.Dto;
using Digital.Net.Api.Controllers.Generic.Crud;
using Digital.Net.Api.Core.Messages;
using Digital.Net.Api.Entities.Context;
using Digital.Net.Api.Entities.Models.Documents;
using Digital.Net.Api.Entities.Models.Users;
using Digital.Net.Api.Entities.Services;
using Digital.Net.Api.Services.Authentication.Attributes;
using Digital.Net.Api.Services.Authentication.Exceptions;
using Digital.Net.Api.Services.Authentication.Services.Authentication;
using Digital.Net.Api.Services.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Digital.Net.Api.Controllers.Controllers.UserApi;

[ApiController, Route("user"), Authorize(AuthorizeType.Jwt)]
public class UserController(
    IEntityService<User, DigitalContext> entityService,
    IUserService userService,
    IUserContextService userContextService,
    IEntityValidator<DigitalContext> entityValidator
) : CrudController<User, DigitalContext, UserDto, UserDto>(entityService, entityValidator)
{
    [HttpGet("self")]
    public ActionResult<Result<UserDto>> GetSelfAsync() =>
        userContextService.GetUser() is { } user
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
        if (result.HasErrorOfType<PasswordMalformedException>())
            return BadRequest(result);
        if (result.HasErrorOfType<InvalidCredentialsException>())
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
        var user = userContextService.GetUser();
        return user.Id != id ? null : user;
    }
}