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

[ApiController, Route("page"), Authorize(AuthorizeType.Any)]
public class PagePaginationController(
    IRepository<Page, DigitalContext> pageRepository
) : PaginationController<Page, DigitalContext, PageDto, PageQuery>(pageRepository)
{
    protected override Expression<Func<Page, bool>> Filter(Expression<Func<Page, bool>> predicate, PageQuery query)
    {
        if (!string.IsNullOrEmpty(query.Title))
            predicate = predicate.Add(x => x.Title.StartsWith(query.Title));
        if (query.IsPublished.HasValue)
            predicate = predicate.Add(x => x.IsPublished == query.IsPublished);
        return predicate;
    }
}