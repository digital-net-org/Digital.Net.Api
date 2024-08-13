using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Safari.Net.Data.Entities;
using SafariDigital.Api.Attributes;
using SafariDigital.Data.Models.Database;
using SafariDigital.Data.Models.Dto;
using SafariDigital.Data.Services;
using SafariDigital.Services.HttpContext;
using SafariDigital.Services.Users;
using SafariDigital.Services.Users.Models;

namespace SafariDigital.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController(
    IEntityService<User, UserQuery> entityService,
    IHttpContextService httpContextService,
    IUserService userService) : ControllerBase
{
    [Authorize(Role = EUserRole.User)]
    [HttpGet("")]
    public IActionResult Get([FromQuery] UserQuery query) => Ok(entityService.Get<UserModel>(query));

    [Authorize(Role = EUserRole.User)]
    [HttpGet("{id:guid}")]
    public IActionResult GetById(Guid id) => Ok(entityService.Get<UserModel>(id));

    [Authorize(Role = EUserRole.User)]
    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Patch(Guid id, [FromBody] JsonPatchDocument<User> patch)
    {
        var user = await GetAuthorizedUser(id);
        if (user is null) return Unauthorized();
        var result = await entityService.Patch<UserModel>(patch, id);
        return Ok(result);
    }

    [Authorize(Role = EUserRole.User)]
    [HttpPut("{id:guid}/password")]
    public async Task<IActionResult> UpdatePassword(Guid id, UpdatePasswordRequest request)
    {
        var user = await GetAuthorizedUser(id);
        if (user is null) return Unauthorized();
        var result = await userService.UpdatePassword(user, request.CurrentPassword, request.NewPassword);
        return Ok(result);
    }

    [Authorize(Role = EUserRole.User)]
    [HttpPut("{id:guid}/avatar")]
    public async Task<IActionResult> UpdateAvatar(Guid id, IFormFile avatar) =>
        throw new NotImplementedException();

    private async Task<User?> GetAuthorizedUser(Guid id)
    {
        var user = await httpContextService.GetAuthenticatedUser();
        return user.Id != id ? null : user;
    }
}