using Digital.Net.Core.Messages;
using Digital.Net.Entities.Services;
using Digital.Net.Mvc.Controllers.Crud;
using Microsoft.AspNetCore.Mvc;
using SafariDigital.Api.Attributes;
using SafariDigital.Api.Controllers.ViewApi.Dto;
using SafariDigital.Data.Models.Database.Users;
using SafariDigital.Data.Models.Database.Views;

namespace SafariDigital.Api.Controllers.ViewApi;

[ApiController, Route("view"), Authorize(Role = EUserRole.User)]
public class ViewCrudController(
    IEntityService<View> viewService,
    IHttpContextAccessor httpContextAccessor
)
    : CrudController<View, ViewModel, ViewPayload>(httpContextAccessor, viewService)
{
    [HttpPost("{id:guid}/duplicate")]
    public async Task<ActionResult<Result>> PostDuplicate(Guid id) =>
        // TODO: Implement loop to duplicate with copy prefix
        // Implement duplication method in Digital.Net.Entities
        Ok();
}