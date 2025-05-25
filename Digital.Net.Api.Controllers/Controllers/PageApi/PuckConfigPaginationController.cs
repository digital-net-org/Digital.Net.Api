using System.Linq.Expressions;
using Digital.Net.Api.Controllers.Controllers.PageApi.Dto;
using Digital.Net.Api.Controllers.Generic.Pagination;
using Digital.Net.Api.Core.Predicates;
using Digital.Net.Api.Entities.Context;
using Digital.Net.Api.Entities.Models.Pages;
using Digital.Net.Api.Entities.Repositories;
using Digital.Net.Api.Services.Authentication.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace Digital.Net.Api.Controllers.Controllers.PageApi;

[ApiController, Route("page/config"), Authorize(AuthorizeType.Any)]
public class PuckConfigPaginationController(
    IRepository<PagePuckConfig, DigitalContext> puckConfigRepository
) : PaginationController<PagePuckConfig, DigitalContext, PuckConfigDto, PuckConfigQuery>(puckConfigRepository)
{
    protected override Expression<Func<PagePuckConfig, bool>> Filter(
        Expression<Func<PagePuckConfig, bool>> predicate, PuckConfigQuery query
    )
    {
        if (!string.IsNullOrEmpty(query.Version))
            predicate = predicate.Add(x => x.Version.StartsWith(query.Version));
        return predicate;
    }
}