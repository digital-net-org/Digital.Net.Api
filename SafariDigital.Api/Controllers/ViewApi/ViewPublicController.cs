using Digital.Net.Core.Models;
using Digital.Net.Entities.Repositories;
using Microsoft.AspNetCore.Mvc;
using SafariDigital.Api.Dto.Entities;
using SafariDigital.Data.Models.Views;

namespace SafariDigital.Api.Controllers.ViewApi;

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