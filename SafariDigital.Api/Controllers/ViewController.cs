using System.Text.Json;
using Digital.Net.Core.Messages;
using Digital.Net.Entities.Models;
using Digital.Net.Entities.Services;
using Microsoft.AspNetCore.Mvc;
using SafariDigital.Api.Attributes;
using SafariDigital.Api.Formatters;
using SafariDigital.Data.Models.Database.Users;
using SafariDigital.Data.Models.Database.Views;
using SafariDigital.Data.Models.Dto.Views;
using SafariDigital.Services.Views;
using SafariDigital.Services.Views.Models;

namespace SafariDigital.Api.Controllers;

[ApiController, Route("[controller]")]
public class ViewController(IEntityService<View, ViewQuery> entityService, IViewService viewService) : ControllerBase
{
    [HttpGet(""), Authorize(Role = EUserRole.User)]
    public ActionResult<QueryResult<ViewModel>> Get([FromQuery] ViewQuery query) =>
        Ok(entityService.Get<ViewModel>(query));

    [HttpGet("{id:int}"), Authorize(Role = EUserRole.User)]
    public ActionResult<Result<ViewModel>> GetById(int id) => Ok(entityService.Get<ViewModel>(id));

    [HttpPatch("{id:int}"), Authorize(Role = EUserRole.User)]
    public async Task<ActionResult<Result<ViewModel>>> Patch(int id, [FromBody] JsonElement patch)
    {
        var result = await entityService.Patch<ViewModel>(JsonPatchFormatter.GetPatchDocument<View>(patch), id);
        return Ok(result);
    }

    [HttpPost("duplicate"), Authorize(Role = EUserRole.User)]
    public async Task<ActionResult<Result>> PostDuplicate([FromBody] DuplicateViewRequest request)
    {
        var result = await viewService.TryViewDuplicateAsync(request.Title);
        return Ok(result);
    }

    [HttpPost(""), Authorize(Role = EUserRole.User)]
    public async Task<ActionResult<ViewModel>> Post([FromBody] CreateViewRequest request)
    {
        var result = await viewService.CreateViewAsync(request);
        return result.HasError ? BadRequest(result) : Ok(result);
    }

    [HttpDelete("{id:int}"), Authorize(Role = EUserRole.User)]
    public async Task<ActionResult<Result>> Delete(int id)
    {
        var result = await viewService.DeleteViewAsync(id);
        return Ok(result);
    }
}