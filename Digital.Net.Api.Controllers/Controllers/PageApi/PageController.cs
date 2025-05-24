using Digital.Net.Api.Controllers.Controllers.PageApi.Dto;
using Digital.Net.Api.Controllers.Generic.Crud;
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
    IEntityService<Page, DigitalContext> pageEntityService
) : CrudController<Page, DigitalContext, PageDto, PagePayload>(pageEntityService)
{
    [HttpGet("{*path}")]
    public ActionResult<Page> GetPublicPage(string path)
    {
        var page = pageRepository
            .Get(v => v.Path == path && v.IsPublished)
            .FirstOrDefault();
        var result = page?.View?.Data is null ? null : new PagePublicDto(page);
        return result is not null ? Ok(result) : NotFound();
    }
}