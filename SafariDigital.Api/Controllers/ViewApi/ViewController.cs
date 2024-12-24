using Digital.Net.Authentication.Attributes;
using Digital.Net.Core.Messages;
using Digital.Net.Entities.Services;
using Digital.Net.Mvc.Controllers.Crud;
using Microsoft.AspNetCore.Mvc;
using SafariDigital.Api.Attributes;
using SafariDigital.Api.Dto.Entities;
using SafariDigital.Data.Models.Views;

namespace SafariDigital.Api.Controllers.ViewApi;

[ApiController, Route("view"), Authorize(AuthorizeType.Jwt)]
public class ViewController(
    IEntityService<View> viewService
) : CrudController<View, ViewModel, ViewPayload>(viewService)
{
    [HttpPost("{id:guid}/duplicate")]
    public async Task<ActionResult<Result>> PostDuplicate(Guid id) =>
        // TODO: Implement loop to duplicate with copy prefix
        // Implement duplication method in Digital.Net.Entities
        throw new NotImplementedException();
}