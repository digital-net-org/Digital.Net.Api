using Microsoft.AspNetCore.Mvc;
using SafariDigital.Api.Attributes;
using SafariDigital.Database.Models.UserTable;
using SafariDigital.DataIdentities.Models.User;
using SafariDigital.DataIdentities.Pagination.User;
using SafariDigital.Services.CrudService;
using SafariDigital.Services.HttpContextService;
using SafariDigital.Services.PaginationService;
using SafariDigital.Services.UserService;
using SafariDigital.Services.UserService.Models;

namespace SafariDigital.Api.Controllers;

[ApiController]
public class UserController(
    IPaginationService<UserPublicModel, UserQuery> paginationService,
    IHttpContextService httpContextService,
    ICrudService<User, UserPublicModel, UserQuery> crudService,
    IUserService userService
) : ControllerBase
{
    [Authorize(Role = EUserRole.User)]
    [HttpGet("[controller]")]
    public IActionResult Get([FromQuery] UserQuery query) => Ok(paginationService.Get(query));

    [Authorize(Role = EUserRole.User)]
    [HttpGet("[controller]/{id}")]
    public async Task<IActionResult> GetById(string id) => Ok(await crudService.GetById(id));

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