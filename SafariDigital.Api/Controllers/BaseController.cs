using Microsoft.AspNetCore.Mvc;
using SafariDigital.Api.Attributes;
using SafariDigital.Database.Models.UserTable;
using SafariDigital.Services.HttpContextService;
using SafariDigital.Services.PaginationService;
using SafariDigital.Services.PaginationService.Models;

namespace SafariDigital.Api.Controllers;

public class BaseController<TModel, TQuery>(
    IHttpContextService httpContextService,
    IPaginationService<TModel, TQuery> paginationService
) : ControllerBase where TModel : class where TQuery : PaginationQuery
{
    [Authorize(Role = EUserRole.User)]
    [HttpGet("[controller]")]
    public async Task<IActionResult> Get([FromQuery] TQuery query) => Ok(await paginationService.Get(query));
}