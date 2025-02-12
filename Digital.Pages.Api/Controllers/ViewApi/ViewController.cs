using Digital.Pages.Api.Attributes;
using Digital.Lib.Net.Authentication.Attributes;
using Digital.Lib.Net.Core.Messages;
using Digital.Lib.Net.Entities.Services;
using Digital.Lib.Net.Mvc.Controllers.Crud;
using Digital.Pages.Api.Dto.Entities;
using Microsoft.AspNetCore.Mvc;
using Digital.Pages.Data.Models.Views;

namespace Digital.Pages.Api.Controllers.ViewApi;

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