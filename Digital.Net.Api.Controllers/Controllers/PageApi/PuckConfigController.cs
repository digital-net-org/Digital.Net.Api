using Digital.Net.Api.Controllers.Controllers.PageApi.Dto;
using Digital.Net.Api.Core.Exceptions;
using Digital.Net.Api.Core.Messages;
using Digital.Net.Api.Entities.Context;
using Digital.Net.Api.Entities.Models.Pages;
using Digital.Net.Api.Entities.Repositories;
using Digital.Net.Api.Entities.Services;
using Digital.Net.Api.Services.Authentication.Attributes;
using Digital.Net.Api.Services.Authentication.Services.Authentication;
using Digital.Net.Api.Services.HttpContext;
using Digital.Net.Api.Services.Pages;
using Digital.Net.Api.Services.Pages.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Digital.Net.Api.Controllers.Controllers.PageApi;

[ApiController, Route("page/config"), Authorize(AuthorizeType.Any)]
public class PuckConfigController(
    IPuckConfigService puckConfigService,
    IHttpCacheService httpCacheService,
    IRepository<PagePuckConfig, DigitalContext> puckConfigRepository,
    IEntityService<PagePuckConfig, DigitalContext> puckConfigEntityService, 
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
        var result = httpCacheService.GetCachedDocument(
            puckConfigRepository.Get(x => x.Version == version).FirstOrDefault()?.Document,
            "application/javascript"
        );
        if (result.HasError)
            return NotFound();
        if (result.Value is null)
            return StatusCode(304);
        return result.Value;
    }
    
    [HttpGet("{id:int}")]
    public ActionResult<Result<PuckConfigDto>> GetConfig(int id)
    {
        var result = puckConfigEntityService.Get<PuckConfigDto>(id);
        return result.HasError ? NotFound(result) : Ok(result);
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
