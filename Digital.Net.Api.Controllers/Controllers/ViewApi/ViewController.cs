using System.Linq.Dynamic.Core;
using Digital.Net.Api.Controllers.Controllers.ViewApi.Dto;
using Digital.Net.Api.Controllers.Generic.Crud;
using Digital.Net.Api.Core.Messages;
using Digital.Net.Api.Core.Models;
using Digital.Net.Api.Entities.Context;
using Digital.Net.Api.Entities.Models.PuckConfigs;
using Digital.Net.Api.Entities.Models.Views;
using Digital.Net.Api.Entities.Repositories;
using Digital.Net.Api.Entities.Services;
using Digital.Net.Api.Services.Authentication.Attributes;
using Digital.Net.Api.Services.Views.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Digital.Net.Api.Controllers.Controllers.ViewApi;

[ApiController, Route("view"), Authorize(AuthorizeType.Any)]
public class ViewController(
    IEntityService<View, DigitalContext> viewEntityService,
    IRepository<PuckConfig, DigitalContext> puckConfigRepository
) : CrudController<View, DigitalContext, ViewDto, ViewPayload>(viewEntityService)
{
    private readonly IEntityService<View, DigitalContext> _viewEntityService = viewEntityService;

    [HttpPost("")]
    public override async Task<ActionResult<Result>> Post([FromBody] ViewPayload payload)
    {
        if (payload.PuckConfigId == 0)
        {
            var defaultVersion = puckConfigRepository
                .Get()
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefault()?.Id;
            if (defaultVersion is null)
                return BadRequest(new Result().AddError(new NoPuckConfigException()));
            
            payload.PuckConfigId = (int)defaultVersion;
        }
        var result = await _viewEntityService.Create(Mapper.Map<ViewPayload, View>(payload));
        return result.HasError ? BadRequest(result) : Ok(result);
    }
};