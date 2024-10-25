using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Safari.Net.Core.Messages;
using Safari.Net.Data.Entities;
using SafariDigital.Api.Attributes;
using SafariDigital.Api.Formatters;
using SafariDigital.Data.Models.Database.Frames;
using SafariDigital.Data.Models.Database.Users;
using SafariDigital.Data.Models.Dto.Frames;
using SafariDigital.Data.Services;
using SafariDigital.Services.Frames;
using SafariDigital.Services.Frames.Models;

namespace SafariDigital.Api.Controllers;

[ApiController, Route("[controller]")]
public class FrameController(IEntityService<Frame, FrameQuery> entityService, IFrameService frameService)
    : ControllerBase
{
    [HttpGet(""), Authorize(Role = EUserRole.User)]
    public ActionResult<QueryResult<FrameModel>> Get([FromQuery] FrameQuery query) =>
        Ok(entityService.Get<FrameModel>(query));

    [HttpGet("{id:int}")]
    public ActionResult<Result<FrameModel>> GetById(int id) => Ok(entityService.Get<FrameModel>(id));

    [HttpPatch("{id:int}"), Authorize(Role = EUserRole.User)]
    public async Task<ActionResult<Result<FrameModel>>> Patch(int id, [FromBody] JsonElement patch)
    {
        var result = await entityService.Patch<FrameModel>(JsonPatchFormatter.GetPatchDocument<Frame>(patch), id);
        return Ok(result);
    }

    [HttpPost("duplicate"), Authorize(Role = EUserRole.User)]
    public async Task<ActionResult<Result>> PostDuplicate([FromBody] DuplicateFrameRequest request)
    {
        var result = await frameService.TryFrameDuplicateAsync(request.Name);
        return Ok(result);
    }

    [HttpPost(""), Authorize(Role = EUserRole.User)]
    public async Task<ActionResult<FrameModel>> Post([FromBody] CreateFrameRequest request)
    {
        var result = await frameService.CreateFrameAsync(request.Name, request.ViewId);
        return result.HasError ? BadRequest(result) : Ok(result);
    }

    [HttpDelete("{id:int}"), Authorize(Role = EUserRole.User)]
    public async Task<ActionResult<Result>> Delete(int id)
    {
        var result = await frameService.DeleteFrameAsync(id);
        return Ok(result);
    }
}