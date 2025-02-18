using System.Linq.Expressions;
using Digital.Lib.Net.Authentication.Attributes;
using Digital.Lib.Net.Core.Predicates;
using Digital.Lib.Net.Entities.Repositories;
using Digital.Lib.Net.Mvc.Controllers.Pagination;
using Digital.Pages.Api.Data;
using Digital.Pages.Api.Data.Views;
using Microsoft.AspNetCore.Mvc;

namespace Digital.Pages.Api.Controllers.ViewApi;

[ApiController, Route("view"), Authorize(AuthorizeType.Jwt)]
public class ViewPaginationController(
    IRepository<View, DigitalPagesContext> viewRepository
) : PaginationController<View, DigitalPagesContext, ViewDto, ViewQuery>(viewRepository)
{
    protected override Expression<Func<View, bool>> Filter(Expression<Func<View, bool>> predicate, ViewQuery query)
    {
        if (!string.IsNullOrEmpty(query.Title))
            predicate = predicate.Add(x => x.Title.StartsWith(query.Title));
        if (query.IsPublished.HasValue)
            predicate = predicate.Add(x => x.IsPublished == query.IsPublished);
        return predicate;
    }
}