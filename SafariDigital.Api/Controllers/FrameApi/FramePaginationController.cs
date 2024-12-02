using System.Linq.Expressions;
using Digital.Net.Core.Predicates;
using Digital.Net.Entities.Repositories;
using Digital.Net.Mvc.Controllers.Pagination;
using Microsoft.AspNetCore.Mvc;
using SafariDigital.Api.Attributes;
using SafariDigital.Api.Controllers.FrameApi.Dto;
using SafariDigital.Data.Models.Database.Frames;
using SafariDigital.Data.Models.Database.Users;

namespace SafariDigital.Api.Controllers.FrameApi;

[ApiController, Route("frame"), Authorize(Role = EUserRole.User)]
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