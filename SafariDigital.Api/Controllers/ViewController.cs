using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Safari.Net.Data.Entities;
using SafariDigital.Api.Attributes;
using SafariDigital.Api.Formatters;
using SafariDigital.Data.Models.Database;
using SafariDigital.Data.Models.Dto.Views;
using SafariDigital.Data.Services;
using SafariDigital.Services.Views;
using SafariDigital.Services.Views.Models;

namespace SafariDigital.Api.Controllers;

[ApiController, Route("[controller]")]
public class ViewController(IEntityService<View, ViewQuery> entityService, IViewService viewService) : ControllerBase
{
    [HttpGet(""), Authorize(Role = EUserRole.User)]
    public IActionResult Get([FromQuery] ViewQuery query) => Ok(entityService.Get<ViewModel>(query));

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id) => Ok(entityService.Get<ViewModel>(id));

    [HttpPatch("{id:int}"), Authorize(Role = EUserRole.User)]
    public async Task<IActionResult> Patch(int id, [FromBody] JsonElement patch)
    {
        var result = await entityService.Patch<ViewModel>(JsonPatchFormatter.GetPatchDocument<View>(patch), id);
        return Ok(result);
    }

    [HttpPost(""), Authorize(Role = EUserRole.User)]
    public async Task<IActionResult> Post([FromBody] CreateViewRequest request)
    {
        var result = await viewService.CreateAsync(request);
        return Ok(result);
    }
}