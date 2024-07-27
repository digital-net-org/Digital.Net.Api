using Microsoft.AspNetCore.Mvc;
using SafariDigital.Api.Attributes;
using SafariDigital.Database.Models.UserTable;
using SafariDigital.DataIdentities.Models.User;
using SafariDigital.DataIdentities.Pagination.User;
using SafariDigital.Services.CrudService;
using SafariDigital.Services.PaginationService;

namespace SafariDigital.Api.Controllers.UserController;

[ApiController]
public class UserController(
    IPaginationService<UserPublicModel, UserQuery> paginationService,
    ICrudService<User, UserPublicModel, UserQuery> crudService
) : ControllerBase
{
    [Authorize(Role = EUserRole.User)]
    [HttpGet("[controller]")]
    public IActionResult Get([FromQuery] UserQuery query) => Ok(paginationService.Get(query));

    [Authorize(Role = EUserRole.User)]
    [HttpGet("[controller]/{id}")]
    public async Task<IActionResult> GetById(string id) => Ok(await crudService.GetById(id));
}