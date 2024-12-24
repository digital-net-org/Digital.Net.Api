using System.Linq.Expressions;
using Digital.Net.Authentication.Attributes;
using Digital.Net.Core.Predicates;
using Digital.Net.Entities.Repositories;
using Digital.Net.Mvc.Controllers.Pagination;
using Microsoft.AspNetCore.Mvc;
using SafariDigital.Api.Attributes;
using SafariDigital.Api.Dto.Entities;
using SafariDigital.Data.Models.Views;

namespace SafariDigital.Api.Controllers.ViewApi;

[ApiController, Route("view"), Authorize(AuthorizeType.Jwt)]
public class ViewPaginationController(
    IRepository<View> viewRepository
) : PaginationController<View, ViewModel, ViewQuery>(viewRepository)
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