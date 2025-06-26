using System.Text.Json;
using Digital.Net.Api.Core.Exceptions;
using Digital.Net.Api.Core.Formatters;
using Digital.Net.Api.Core.Messages;
using Digital.Net.Api.Core.Models;
using Digital.Net.Api.Entities.Exceptions;
using Digital.Net.Api.Entities.Models;
using Digital.Net.Api.Entities.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Api.Controllers.Generic.Crud;

[Route("[controller]")]
public abstract class CrudController<T, TContext, TDto, TPayload>(
    IEntityService<T, TContext> entityService,
    IEntityValidator<TContext> entityValidator
) : ControllerBase
    where T : Entity
    where TContext : DbContext
    where TDto : class
    where TPayload : class
{
    [HttpGet("schema")]
    public ActionResult<Result<List<SchemaProperty<T>>>> GetSchema() =>
        Ok(new Result<List<SchemaProperty<T>>>(entityValidator.GetSchema<T>()));

    [HttpGet("{id:guid}")]
    public virtual ActionResult<Result<TDto>> GetById(Guid id)
    {
        var result = entityService.Get<TDto>(id);
        return result.HasError ? NotFound(result) : Ok(result);
    }

    [HttpPost("")]
    public virtual async Task<ActionResult<Result>> Post([FromBody] TPayload payload)
    {
        var result = await entityService.Create(Mapper.Map<TPayload, T>(payload));
        return result.HasError ? BadRequest(result) : Ok(result);
    }

    [HttpPatch("{id:guid}")]
    public virtual async Task<ActionResult<Result>> Patch(Guid id, [FromBody] JsonElement patch)
    {
        var result = await entityService.Patch(JsonFormatter.GetPatchDocument<T>(patch), id);
        if (result.HasError && result.HasErrorOfType<ResourceNotFoundException>())
            return NotFound(result);
        if (result.HasError && (
                result.HasErrorOfType<InvalidOperationException>() 
                || result.HasErrorOfType<EntityValidationException>()
        ))
            return BadRequest(result);
        if (result.HasError)
            return StatusCode(500, result);

        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public virtual async Task<ActionResult<Result>> Delete(Guid id)
    {
        var result = await entityService.Delete(id);
        return result.HasError ? NotFound(result) : Ok(result);
    }
}