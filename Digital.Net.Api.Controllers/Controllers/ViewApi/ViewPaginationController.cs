using System.Linq.Expressions;
using Digital.Net.Api.Controllers.Controllers.ViewApi.Dto;
using Digital.Net.Api.Controllers.Generic.Pagination;
using Digital.Net.Api.Core.Predicates;
using Digital.Net.Api.Entities.Context;
using Digital.Net.Api.Entities.Models.Views;
using Digital.Net.Api.Entities.Repositories;
using Digital.Net.Api.Services.Authentication.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace Digital.Net.Api.Controllers.Controllers.ViewApi;

[ApiController, Route("view"), Authorize(AuthorizeType.Any)]
public class ViewPaginationController(
    IRepository<View, DigitalContext> viewRepository
) : PaginationController<View, DigitalContext, ViewLightDto, ViewQuery>(viewRepository)
{
    protected override Expression<Func<View, bool>> Filter(Expression<Func<View, bool>> predicate, ViewQuery query)
    {
        if (!string.IsNullOrEmpty(query.Name))
            predicate = predicate.Add(x => x.Name.StartsWith(query.Name));
        return predicate;
    }
}