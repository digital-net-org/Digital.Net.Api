using System.Text.Json;
using Digital.Lib.Net.Authentication.Attributes;
using Digital.Lib.Net.Authentication.Services.Authentication.ApiUsers;
using Digital.Lib.Net.Core.Messages;
using Digital.Lib.Net.Entities.Services;
using Digital.Lib.Net.Mvc.Controllers.Crud;
using Microsoft.AspNetCore.Mvc;
using SafariDigital.Api.Attributes;
using SafariDigital.Api.Dto.Entities;
using SafariDigital.Api.Dto.Payloads.UserApi;
using SafariDigital.Data.Models.Documents;
using SafariDigital.Data.Models.Users;
using SafariDigital.Services.Users;

namespace SafariDigital.Api.Controllers.UserApi;

[ApiController, Route("user"), Authorize(AuthorizeType.Jwt)]
public class UserController(
    IEntityService<User> entityService,
    IApiUserService<User> apiUserService,
    IUserService userService) : CrudController<User, UserModel, UserModel>(entityService)
{
    [HttpPatch("{id}")]
    public override async Task<ActionResult<Result>> Patch(string id, [FromBody] JsonElement patch)
    {
        var user = await GetAuthorizedUser(Guid.Parse(id));
        if (user is null)
            return Unauthorized();

        return await base.Patch(id, patch);
    }

    [NonAction]
    public override async Task<ActionResult<Result>> Post([FromBody] UserModel payload) => NotFound();

    [NonAction]
    public override async Task<ActionResult<Result>> Delete(string id) => NotFound();

    [HttpPut("{id:guid}/password")]
    public async Task<ActionResult<Result>> UpdatePassword([FromBody] UpdatePasswordPayload request, Guid id)
    {
        var user = await GetAuthorizedUser(id);
        if (user is null)
            return Unauthorized();
        return await userService.UpdatePassword(user, request.CurrentPassword, request.NewPassword);
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
        var user = await apiUserService.GetAuthenticatedUserAsync();
        return user?.Id != id ? null : user;
    }
}