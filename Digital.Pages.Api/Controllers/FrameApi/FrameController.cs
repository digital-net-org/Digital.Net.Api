using Digital.Lib.Net.Authentication.Attributes;
using Digital.Lib.Net.Entities.Services;
using Digital.Lib.Net.Mvc.Controllers.Crud;
using Digital.Pages.Api.Data;
using Digital.Pages.Api.Data.Frames;
using Microsoft.AspNetCore.Mvc;

namespace Digital.Pages.Api.Controllers.FrameApi;

[ApiController, Route("frame"), Authorize(AuthorizeType.Jwt)]
public class FrameController(
    IEntityService<Frame, DigitalPagesContext> frameService
) : CrudController<Frame, DigitalPagesContext, FrameDto, FramePayload>(frameService);