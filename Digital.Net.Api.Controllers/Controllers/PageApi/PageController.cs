using Digital.Net.Api.Controllers.Controllers.PageApi.Dto;
using Digital.Net.Api.Controllers.Generic.Crud;
using Digital.Net.Api.Core.Messages;
using Digital.Net.Api.Core.Models;
using Digital.Net.Api.Entities.Context;
using Digital.Net.Api.Entities.Models.Pages;
using Digital.Net.Api.Entities.Repositories;
using Digital.Net.Api.Entities.Services;
using Digital.Net.Api.Services.Authentication.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace Digital.Net.Api.Controllers.Controllers.PageApi;

[ApiController, Route("page"), Authorize(AuthorizeType.Any)]
public class PageController(
    IRepository<Page, DigitalContext> pageRepository,
    IRepository<PageMeta, DigitalContext> pageMetaRepository,
    IEntityService<Page, DigitalContext> pageEntityService
) : CrudController<Page, DigitalContext, PageDto, PagePayload>(pageEntityService)
{
    private readonly IEntityService<Page, DigitalContext> _pageEntityService = pageEntityService;

    [HttpGet("path/{*path}")]
    public ActionResult<Page> GetPageByPath(string path)
    {
        var page = pageRepository
            .Get(p => p.Path == path && p.IsPublished)
            .FirstOrDefault();
        var result = page?.PuckData is null ? null : new PageDto(page);
        return result is not null ? Ok(result) : NotFound();
    }
    
    [HttpGet("{id:Guid}/meta")]
    public ActionResult<List<PageMeta>> GetMetaByPageId(Guid id)
    {
        var metas = pageMetaRepository
            .Get(p => p.Page.Id == id && p.Page.IsPublished)
            .Select(p => new PageMetaDto(p))
            .ToList();
        return Ok(metas);
    }
    
    [HttpPost("")]
    public override async Task<ActionResult<Result>> Post([FromBody] PagePayload payload)
    {
        var result = await _pageEntityService.Create(Mapper.Map<PagePayload, Page>(payload));
        return result.HasError ? BadRequest(result) : Ok(result);
    }
}