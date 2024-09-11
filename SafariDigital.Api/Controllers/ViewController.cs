using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Safari.Net.Core.Messages;
using Safari.Net.Data.Entities;
using SafariDigital.Api.Attributes;
using SafariDigital.Api.Formatters;
using SafariDigital.Data.Models.Database;
using SafariDigital.Data.Models.Dto.Views;
using SafariDigital.Data.Services;
using SafariDigital.Services.Views;
using SafariDigital.Services.Views.Models;

namespace SafariDigital.Api.Controllers;

[ApiController, Route("[controller]")]
public class ViewController(IEntityService<View, ViewQuery> entityService, IViewService viewService) : ControllerBase
{
    [HttpGet(""), Authorize(Role = EUserRole.User)]
    public ActionResult<QueryResult<ViewModel>> Get([FromQuery] ViewQuery query) =>
        Ok(entityService.Get<ViewModel>(query));

    [HttpGet("{id:int}")]
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

    [HttpPost("duplicate/frame"), Authorize(Role = EUserRole.User)]
    public async Task<ActionResult<Result>> PostDuplicateFrame([FromBody] DuplicateViewFrameRequest request)
    {
        var result = await viewService.TryViewFrameDuplicateAsync(request.Name);
        return Ok(result);
    }

    [HttpPost(""), Authorize(Role = EUserRole.User)]
    public async Task<ActionResult<ViewModel>> Post([FromBody] CreateViewRequest request)
    {
        var result = await viewService.CreateViewAsync(request);
        return Ok(result);
    }

    [HttpDelete("{id:int}"), Authorize(Role = EUserRole.User)]
    public async Task<ActionResult<Result>> Delete(int id)
    {
        var result = await viewService.DeleteViewAsync(id);
        return Ok(result);
    }

    [HttpGet("{id:int}/frame/{frameId:int}"), Authorize(Role = EUserRole.User)]
    public async Task<ActionResult<Result<FrameModel>>> GetFrame(int id, int frameId)
    {
        var result = await viewService.GetViewFrameAsync(id, frameId);
        return Ok(result);
    }

    [HttpPost("{id:int}/frame"), Authorize(Role = EUserRole.User)]
    public async Task<ActionResult<Result<FrameModel>>> PostFrame(int id, [FromBody] CreateViewFrameRequest request)
    {
        var result = await viewService.CreateViewFrameAsync(id, request.Name);
        return Ok(result);
    }

    [HttpDelete("{id:int}/frame/{frameId:int}"), Authorize(Role = EUserRole.User)]
    public async Task<ActionResult<Result>> DeleteFrame(int id, int frameId)
    {
        var result = await viewService.DeleteViewFrameAsync(id, frameId);
        return Ok(result);
    }

    [HttpPatch("{id:int}/frame/{frameId:int}"), Authorize(Role = EUserRole.User)]
    public async Task<ActionResult<Result<FrameModel>>> PatchFrame(int id, int frameId, [FromBody] JsonElement patch)
    {
        var result = await viewService.PatchViewFrameAsync(
            id,
            frameId,
            JsonPatchFormatter.GetPatchDocument<ViewFrame>(patch)
        );
        return Ok(result);
    }
}