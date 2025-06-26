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
    IEntityService<Page, DigitalContext> pageEntityService,
    IEntityValidator<DigitalContext> entityValidator
) : CrudController<Page, DigitalContext, PageDto, PagePayload>(pageEntityService, entityValidator)
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
    public ActionResult<Result<List<PageMetaDto>>> GetMetaByPageId(Guid id)
    {
        var result = new Result<List<PageMetaDto>>
        {
            Value = pageMetaRepository
                .Get(p => p.Page.Id == id)
                .OrderBy(p => p.CreatedAt)
                .Select(p => new PageMetaDto(p))
                .ToList()
        };
        return Ok(result);
    }
    
    [HttpPost("")]
    public override async Task<ActionResult<Result>> Post([FromBody] PagePayload payload)
    {
        var result = await _pageEntityService.Create(Mapper.Map<PagePayload, Page>(payload));
        return result.HasError ? BadRequest(result) : Ok(result);
    }
}