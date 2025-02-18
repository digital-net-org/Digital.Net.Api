using Digital.Lib.Net.Core.Models;
using Digital.Lib.Net.Entities.Repositories;
using Digital.Pages.Api.Data;
using Digital.Pages.Api.Data.Views;
using Microsoft.AspNetCore.Mvc;

namespace Digital.Pages.Api.Controllers.ViewApi;

[ApiController, Route("view")]
public class ViewPublicController(
    IRepository<View, DigitalPagesContext> viewRepository
) : ControllerBase
{
    [HttpGet("public/{*path}")]
    public ActionResult<View> GetPublicView(string path)
    {
        var view = viewRepository
            .Get(v => v.Path == path && v.IsPublished)
            .FirstOrDefault();

        if (view is null)
            return NotFound();

        var result = Mapper.MapFromConstructor<View, ViewPublicDto>(view);
        return result.Data is not null ? Ok(result) : NotFound();
    }
}