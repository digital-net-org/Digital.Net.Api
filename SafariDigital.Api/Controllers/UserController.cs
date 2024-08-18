using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Safari.Net.Data.Entities;
using SafariDigital.Api.Attributes;
using SafariDigital.Api.Formatters;
using SafariDigital.Data.Models.Database;
using SafariDigital.Data.Models.Dto.Users;
using SafariDigital.Data.Services;
using SafariDigital.Services.HttpContext;
using SafariDigital.Services.Users;
using SafariDigital.Services.Users.Models;

namespace SafariDigital.Api.Controllers;

[ApiController, Route("[controller]")]
public class UserController(
    IEntityService<User, UserQuery> entityService,
    IHttpContextService httpContextService,
    IUserService userService) : ControllerBase
{
    [HttpGet(""), Authorize(Role = EUserRole.User)]
    public IActionResult Get([FromQuery] UserQuery query) => Ok(entityService.Get<UserModel>(query));

    [HttpGet("{id:guid}"), Authorize(Role = EUserRole.User)]
    public IActionResult GetById(Guid id) => Ok(entityService.Get<UserModel>(id));

    [HttpPatch("{id:guid}"), Authorize(Role = EUserRole.User)]
    public async Task<IActionResult> Patch(Guid id, [FromBody] JsonElement patch)
    {
        var user = await GetAuthorizedUser(id);
        if (user is null) return Unauthorized();
        var result = await entityService.Patch<UserModel>(JsonPatchFormatter.GetPatchDocument<User>(patch), id);
        return Ok(result);
    }

    [HttpPut("{id:guid}/password"), Authorize(Role = EUserRole.User)]
    public async Task<IActionResult> UpdatePassword(Guid id, UpdatePasswordRequest request)
    {
        var user = await GetAuthorizedUser(id);
        if (user is null) return Unauthorized();
        var result = await userService.UpdatePassword(user, request.CurrentPassword, request.NewPassword);
        return Ok(result);
    }

    [HttpPut("{id:guid}/avatar"), Authorize(Role = EUserRole.User)]
    public async Task<IActionResult> UpdateAvatar(Guid id, IFormFile avatar)
    {
        var user = await GetAuthorizedUser(id);
        if (user is null) return Unauthorized();
        var result = await userService.UpdateAvatar(user, avatar);
        return Ok(result);
    }

    [HttpDelete("{id:guid}/avatar"), Authorize(Role = EUserRole.User)]
    public async Task<IActionResult> RemoveAvatar(Guid id)
    {
        var user = await GetAuthorizedUser(id);
        if (user is null) return Unauthorized();
        var result = await userService.RemoveUserAvatar(user);
        return Ok(result);
    }

    private async Task<User?> GetAuthorizedUser(Guid id)
    {
        var user = await httpContextService.GetAuthenticatedUser();
        return user.Id != id ? null : user;
    }
}