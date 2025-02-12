using Digital.Lib.Net.Core.Models;
using Digital.Lib.Net.Entities.Repositories;
using Digital.Pages.Api.Dto.Entities;
using Microsoft.AspNetCore.Mvc;
using Digital.Pages.Data.Models.Views;

namespace Digital.Pages.Api.Controllers.ViewApi;

[ApiController, Route("view")]
public class ViewPublicController(
    IRepository<View> viewRepository
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

        var result = Mapper.MapFromConstructor<View, ViewPublicModel>(view);
        return result.Data is not null ? Ok(result) : NotFound();
    }
}