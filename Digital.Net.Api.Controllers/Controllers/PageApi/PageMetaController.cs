using Digital.Net.Api.Controllers.Controllers.PageApi.Dto;
using Digital.Net.Api.Controllers.Generic.Crud;
using Digital.Net.Api.Core.Messages;
using Digital.Net.Api.Entities.Context;
using Digital.Net.Api.Entities.Models.Pages;
using Digital.Net.Api.Entities.Services;
using Digital.Net.Api.Services.Authentication.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace Digital.Net.Api.Controllers.Controllers.PageApi;

[ApiController, Route("page/meta"), Authorize(AuthorizeType.Any)]
public class PageMetaController(
    IEntityService<PageMeta, DigitalContext> pageMetaEntityService
) : CrudController<PageMeta, DigitalContext, PageMetaDto, PageMetaPayload>(pageMetaEntityService)
{
    [HttpGet("{id}")]
    public override ActionResult<Result<PageMetaDto>> GetById(string id) => NotFound();
}