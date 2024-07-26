using Microsoft.AspNetCore.Mvc;
using SafariDigital.Database.Models.UserTable;
using SafariDigital.Services.HttpContextService;
using SafariDigital.Services.PaginationService;
using SafariDigital.Services.PaginationService.Services;

namespace SafariDigital.Api.Controllers.UserController;

[ApiController]
public class UserController(
    IHttpContextService httpContextService,
    IPaginationService<UserPublicModel, UserQuery> paginationService
) : BaseController<UserPublicModel, UserQuery>(httpContextService, paginationService);