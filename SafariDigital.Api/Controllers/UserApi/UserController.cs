using System.Text.Json;
using Digital.Net.Core.Messages;
using Digital.Net.Entities.Services;
using Microsoft.AspNetCore.Mvc;
using SafariDigital.Api.Attributes;
using SafariDigital.Api.Controllers.UserApi.Dto;
using SafariDigital.Api.Formatters;
using SafariDigital.Data.Models.Database.Documents;
using SafariDigital.Data.Models.Database.Users;
using SafariDigital.Services.Authentication.Service;
using SafariDigital.Services.Users;

namespace SafariDigital.Api.Controllers.UserApi;

[ApiController, Route("user/{id:guid}")]
public class UserController(
    IEntityService<User> entityService,
    IAuthenticatedUserService authenticatedUserService,
    IUserService userService) : ControllerBase
{

    [HttpGet(""), Authorize(Role = EUserRole.User)]
    public ActionResult<Result<UserModel>> GetById(Guid id) => entityService.Get<UserModel>(id);

    [HttpPatch(""), Authorize(Role = EUserRole.User)]
    public async Task<ActionResult<Result>> Patch(Guid id, [FromBody] JsonElement patch)
    {
        var user = await GetAuthorizedUser(id);
        if (user is null) return Unauthorized();
        return await entityService.Patch(JsonPatchFormatter.GetPatchDocument<User>(patch), id);
    }

    [HttpPut("password"), Authorize(Role = EUserRole.User)]
    public async Task<ActionResult<Result>> UpdatePassword([FromBody] UpdatePasswordPayload request, Guid id)
    {
        var user = await GetAuthorizedUser(id);
        if (user is null) return Unauthorized();
        return await userService.UpdatePassword(user, request.CurrentPassword, request.NewPassword);
    }

    [HttpPut("avatar"), Authorize(Role = EUserRole.User)]
    public async Task<ActionResult<Result<Document>>> UpdateAvatar(Guid id, IFormFile avatar)
    {
        var user = await GetAuthorizedUser(id);
        if (user is null) return Unauthorized();
        return await userService.UpdateAvatar(user, avatar);
    }

    [HttpDelete("avatar"), Authorize(Role = EUserRole.User)]
    public async Task<ActionResult<Result>> RemoveAvatar(Guid id)
    {
        var user = await GetAuthorizedUser(id);
        if (user is null) return Unauthorized();
        return await userService.RemoveUserAvatar(user);
    }

    private async Task<User?> GetAuthorizedUser(Guid id)
    {
        var user = await authenticatedUserService.GetAuthenticatedUser();
        return user.Id != id ? null : user;
    }
}