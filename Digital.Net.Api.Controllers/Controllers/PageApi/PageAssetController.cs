using Digital.Net.Api.Controllers.Controllers.PageApi.Dto;
using Digital.Net.Api.Core.Exceptions;
using Digital.Net.Api.Core.Messages;
using Digital.Net.Api.Entities.Context;
using Digital.Net.Api.Entities.Models.Pages;
using Digital.Net.Api.Entities.Services;
using Digital.Net.Api.Services.Authentication.Attributes;
using Digital.Net.Api.Services.Authentication.Services.Authentication;
using Digital.Net.Api.Services.HttpContext;
using Digital.Net.Api.Services.Pages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Digital.Net.Api.Controllers.Controllers.PageApi;

[ApiController, Route("page/asset"), Authorize(AuthorizeType.Any)]
public class PageAssetController(
    IPageAssetService pageAssetService,
    IHttpCacheService httpCacheService,
    IEntityService<PageAsset, DigitalContext> pageAssetEntityService,
    IUserContextService userContextService
) : ControllerBase
{
    [HttpGet("path/{*path}")]
    public ActionResult GetAssetFile(string path)
    {
        var result = httpCacheService
            .GetCachedDocument(pageAssetEntityService.GetFirst<PageAsset>(x => x.Path == path).Value?.Document);
        if (result.HasError)
            return NotFound();
        if (result.Value is null)
            return StatusCode(304);
        return result.Value;
    }

    [HttpGet("{id:int}")]
    public ActionResult<Result<PageAssetDto>> GetAsset(int id)
    {
        var result = pageAssetEntityService.Get<PageAssetDto>(id);
        return result.HasError ? NotFound(result) : Ok(result);
    }

    [HttpPost("upload")]
    public async Task<ActionResult<Result<PageAssetDto>>> UploadAsset(IFormFile file, [FromForm] string path)
    {
        var user = userContextService.GetUser();
        var result = await pageAssetService.UploadAsync(file, path, user);
        if (result.HasErrorOfType<ResourceDuplicateException>())
            return Conflict(result);
        if (result.HasError)
            return BadRequest(result);
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<Result>> DeleteAsset(int id)
    {
        var result = await pageAssetService.DeleteAsync(id);
        if (result.HasErrorOfType<ResourceNotFoundException>())
            return NotFound(result);
        if (result.HasError)
            return StatusCode(500, result);
        return Ok(result);
    }
}