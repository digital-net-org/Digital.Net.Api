using Digital.Net.Core.Messages;
using Digital.Net.Entities.Repositories;
using Digital.Net.Entities.Services;
using Digital.Net.Mvc.Controllers.Crud;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SafariDigital.Api.Attributes;
using SafariDigital.Api.Controllers.FrameApi.Dto;
using SafariDigital.Core.Application;
using SafariDigital.Data.Models.Database.Frames;
using SafariDigital.Data.Models.Database.Users;

namespace SafariDigital.Api.Controllers.FrameApi;

[ApiController, Route("frame"), Authorize(Role = EUserRole.User)]
public class FrameCrudController(
    IRepository<Frame> frameRepository,
    IEntityService<Frame> frameService,
    IHttpContextAccessor httpContextAccessor
)
    : CrudController<Frame, FrameModel, FramePayload>(httpContextAccessor, frameService)
{
    [HttpPost("{id:guid}/duplicate")]
    public async Task<ActionResult<Result>> PostDuplicate(Guid id)
    {
        var result = new Result();
        var source = await frameRepository.GetByIdAsync(id);
        if (source is null)
            return BadRequest(result.AddError(EApplicationMessage.EntityNotFound));
        var frame = await frameRepository.Get(vf => vf.Name == source.Name).FirstOrDefaultAsync();
        if (frame is not null)
            return BadRequest(result.AddError(EApplicationMessage.EntityUniqueViolation));

        // TODO: Implement loop to duplicate with copy prefix
        // Implement duplication method in Digital.Net.Entities
        
        return Ok(result);
    }
}