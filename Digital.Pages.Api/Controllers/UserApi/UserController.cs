using System.Text.Json;
using Digital.Lib.Net.Authentication.Attributes;
using Digital.Lib.Net.Authentication.Services.Authentication.ApiUsers;
using Digital.Lib.Net.Core.Messages;
using Digital.Lib.Net.Entities.Services;
using Digital.Lib.Net.Mvc.Controllers.Crud;
using Digital.Pages.Api.Attributes;
using Digital.Pages.Api.Dto.Entities;
using Digital.Pages.Api.Dto.Payloads.UserApi;
using Microsoft.AspNetCore.Mvc;
using Digital.Pages.Data.Models.Documents;
using Digital.Pages.Data.Models.Users;
using Digital.Pages.Services.Users;

namespace Digital.Pages.Api.Controllers.UserApi;

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