using Microsoft.AspNetCore.Mvc;
using Safari.Net.Data.Entities;
using Safari.Net.Data.Repositories;
using SafariDigital.Api.Attributes;
using SafariDigital.Data.Models.Database;
using SafariDigital.Data.Models.Dto;
using SafariDigital.Data.Services;
using SafariDigital.Services.HttpContextService;
using SafariDigital.Services.UserService;
using SafariDigital.Services.UserService.Models;

namespace SafariDigital.Api.Controllers;

[ApiController]
public class UserController(
    IEntityService<User, UserQuery> entityService,
    IRepository<User> userRepository,
    IHttpContextService httpContextService,
    IUserService userService
) : ControllerBase
{
    [Authorize(Role = EUserRole.User)]
    [HttpGet("[controller]")]
    public IActionResult Get([FromQuery] UserQuery query) => Ok(entityService.Get<UserModel>(query));

    [Authorize(Role = EUserRole.User)]
    [HttpGet("[controller]/{id}")]
    public async Task<IActionResult> GetById(string id) => Ok(await userRepository.GetByIdAsync(id));

    [Authorize(Role = EUserRole.User)]
    [HttpPut("[controller]/{id}/password")]
    public async Task<IActionResult> UpdatePassword(string id, UpdatePasswordRequest request)
    {
        var user = await GetAuthorizedUser(id);
        if (user is null) return Unauthorized();
        var result = await userService.UpdatePassword(user, request.CurrentPassword, request.NewPassword);
        return Ok(result);
    }

    [Authorize(Role = EUserRole.User)]
    [HttpPut("[controller]/{id}/avatar")]
    public async Task<IActionResult> UpdateAvatar(string id, IFormFile avatar) =>
        throw new NotImplementedException();

    private async Task<User?> GetAuthorizedUser(string id)
    {
        var user = await httpContextService.GetAuthenticatedUser();
        return user.Id.ToString() != id ? null : user;
    }
}