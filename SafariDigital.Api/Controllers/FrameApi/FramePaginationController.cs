using System.Linq.Expressions;
using Digital.Net.Authentication.Attributes;
using Digital.Net.Core.Predicates;
using Digital.Net.Entities.Repositories;
using Digital.Net.Mvc.Controllers.Pagination;
using Microsoft.AspNetCore.Mvc;
using SafariDigital.Api.Attributes;
using SafariDigital.Api.Dto.Entities;
using SafariDigital.Data.Models.Frames;

namespace SafariDigital.Api.Controllers.FrameApi;

[ApiController, Route("frame"), Authorize(AuthorizeType.Jwt)]
public class FramePaginationController(
    IRepository<Frame> frameRepository
) : PaginationController<Frame, FrameLightModel, FrameQuery>(frameRepository)
{
    protected override Expression<Func<Frame, bool>> Filter(Expression<Func<Frame, bool>> predicate, FrameQuery query)
    {
        if (!string.IsNullOrEmpty(query.Name))
            predicate = predicate.Add(x => x.Name.StartsWith(query.Name));
        return predicate;
    }
}