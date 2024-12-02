using System.Linq.Expressions;
using Digital.Net.Core.Predicates;
using Digital.Net.Entities.Repositories;
using Digital.Net.Mvc.Controllers.Pagination;
using Microsoft.AspNetCore.Mvc;
using SafariDigital.Api.Attributes;
using SafariDigital.Api.Controllers.ViewApi.Dto;
using SafariDigital.Data.Models.Database.Users;
using SafariDigital.Data.Models.Database.Views;

namespace SafariDigital.Api.Controllers.ViewApi;

[ApiController, Route("view"), Authorize(Role = EUserRole.User)]
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