using Digital.Net.Api.Controllers.Controllers.ViewApi.Dto;
using Digital.Net.Api.Core.Exceptions;
using Digital.Net.Api.Core.Messages;
using Digital.Net.Api.Entities.Context;
using Digital.Net.Api.Entities.Models.PuckConfigs;
using Digital.Net.Api.Entities.Repositories;
using Digital.Net.Api.Services.Authentication.Attributes;
using Digital.Net.Api.Services.Authentication.Services.Authentication;
using Digital.Net.Api.Services.HttpContext.Extensions;
using Digital.Net.Api.Services.Views;
using Digital.Net.Api.Services.Views.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Digital.Net.Api.Controllers.Controllers.ViewApi;

[ApiController, Route("view/config"), Authorize(AuthorizeType.Any)]
public class PuckConfigController(
    IPuckConfigService puckConfigService,
    IRepository<PuckConfig, DigitalContext> puckConfigRepository,
    IUserContextService userContextService
) : ControllerBase
{
    [HttpGet("test")]
    public async Task<ActionResult<Result>> Test()
    {
        var result = new Result();
        if (await puckConfigRepository.CountAsync(_ => true) is 0)
            result.AddError(new NoPuckConfigException());
        
        return result.HasErrorOfType<NoPuckConfigException>() ? Conflict(result) : Ok(result);
    }
    
    [HttpGet("version/{version}")]
    public ActionResult GetConfigFile(string version)
    {
        var result = new Result();
        var config = result.Try(() => puckConfigService.GetConfig(version));
        var etag = config?.DocumentId.ToString();
        
        if (result.HasError || config is null)
            return NotFound();
        if (Request.Headers.TestIfNoneMatch(etag))
            return StatusCode(304);

        var file = result.Try(() => puckConfigService.GetConfigFile(config));
        if (result.HasError || file is null)
            return NotFound();
        
        Response.Headers.CacheControl = "public, max-age=0, must-revalidate";
        Response.Headers.ETag = etag;
        Response.Headers.Remove("Content-Disposition");
        return file;
    }
    
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Result<PuckConfigDto>>> GetConfig(int id)
    {
        var result = new Result<PuckConfigDto>();
        var config = await puckConfigRepository.GetByIdAsync(id);
        if (config is null)
            return NotFound(result.AddError(new ResourceNotFoundException()));
        result.Value = new PuckConfigDto(config);
        return Ok(result);
    }

    [HttpPost("upload")]
    public async Task<ActionResult<Result<PuckConfigDto>>> UploadConfig(IFormFile file, [FromForm] string version)
    {
        var user = userContextService.GetUser();
        var result = await puckConfigService.UploadAsync(file, version, user);
        if (result.HasErrorOfType<ResourceDuplicateException>())
            return Conflict(result);
        if (result.HasError)
            return BadRequest(result);
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<Result>> DeleteConfig(int id)
    {
        var result = await puckConfigService.DeleteAsync(id);
        if (result.HasErrorOfType<ResourceNotFoundException>())
            return NotFound(result);
        if (result.HasErrorOfType<CannotDeletePublishedConfigException>())
            return BadRequest(result);
        if (result.HasError)
            return StatusCode(500, result);
        return Ok(result);
    }
};