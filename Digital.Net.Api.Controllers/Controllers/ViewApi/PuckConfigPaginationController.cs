using System.Linq.Expressions;
using Digital.Net.Api.Controllers.Controllers.ViewApi.Dto;
using Digital.Net.Api.Controllers.Generic.Pagination;
using Digital.Net.Api.Core.Predicates;
using Digital.Net.Api.Entities.Context;
using Digital.Net.Api.Entities.Models.PuckConfigs;
using Digital.Net.Api.Entities.Repositories;
using Digital.Net.Api.Services.Authentication.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace Digital.Net.Api.Controllers.Controllers.ViewApi;

[ApiController, Route("view/config"), Authorize(AuthorizeType.Any)]
public class PuckConfigPaginationController(
    IRepository<PuckConfig, DigitalContext> puckConfigRepository
) : PaginationController<PuckConfig, DigitalContext, PuckConfigDto, PuckConfigQuery>(puckConfigRepository)
{
    protected override Expression<Func<PuckConfig, bool>> Filter(
        Expression<Func<PuckConfig, bool>> predicate, PuckConfigQuery query
    )
    {
        if (!string.IsNullOrEmpty(query.Version))
            predicate = predicate.Add(x => x.Version.StartsWith(query.Version));
        return predicate;
    }
}