using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Safari.Net.Core.Messages;
using Safari.Net.Data.Entities;
using SafariDigital.Api.Attributes;
using SafariDigital.Api.Formatters;
using SafariDigital.Data.Models.Database.Documents;
using SafariDigital.Data.Models.Database.Users;
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
    public ActionResult<QueryResult<UserModel>> Get([FromQuery] UserQuery query) =>
        entityService.Get<UserModel>(query);

    [HttpGet("{id:guid}"), Authorize(Role = EUserRole.User)]
    public ActionResult<Result<UserModel>> GetById(Guid id) => entityService.Get<UserModel>(id);

    [HttpPatch("{id:guid}"), Authorize(Role = EUserRole.User)]
    public async Task<ActionResult<Result<UserModel>>> Patch(Guid id, [FromBody] JsonElement patch)
    {
        var user = await GetAuthorizedUser(id);
        if (user is null) return Unauthorized();
        return await entityService.Patch<UserModel>(JsonPatchFormatter.GetPatchDocument<User>(patch), id);
    }

    [HttpPut("{id:guid}/password"), Authorize(Role = EUserRole.User)]
    public async Task<ActionResult<Result>> UpdatePassword(Guid id, UpdatePasswordRequest request)
    {
        var user = await GetAuthorizedUser(id);
        if (user is null) return Unauthorized();
        return await userService.UpdatePassword(user, request.CurrentPassword, request.NewPassword);
    }

    [HttpPut("{id:guid}/avatar"), Authorize(Role = EUserRole.User)]
    public async Task<ActionResult<Result<Document>>> UpdateAvatar(Guid id, IFormFile avatar)
    {
        var user = await GetAuthorizedUser(id);
        if (user is null) return Unauthorized();
        return await userService.UpdateAvatar(user, avatar);
    }

    [HttpDelete("{id:guid}/avatar"), Authorize(Role = EUserRole.User)]
    public async Task<ActionResult<Result>> RemoveAvatar(Guid id)
    {
        var user = await GetAuthorizedUser(id);
        if (user is null) return Unauthorized();
        return await userService.RemoveUserAvatar(user);
    }

    private async Task<User?> GetAuthorizedUser(Guid id)
    {
        var user = await httpContextService.GetAuthenticatedUser();
        return user.Id != id ? null : user;
    }
}