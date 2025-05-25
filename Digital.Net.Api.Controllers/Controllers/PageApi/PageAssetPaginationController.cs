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

[ApiController, Route("page/asset"), Authorize(AuthorizeType.Any)]
public class PageAssetPaginationController(
    IRepository<PageAsset, DigitalContext> pageAssetRepository
) : PaginationController<PageAsset, DigitalContext, PageAssetLightDto, PageAssetQuery>(pageAssetRepository)
{
    protected override Expression<Func<PageAsset, bool>> Filter(
        Expression<Func<PageAsset, bool>> predicate, PageAssetQuery query
    )
    {
        if (!string.IsNullOrEmpty(query.Path))
            predicate = predicate.Add(x => x.Path.Contains(query.Path));
        if (!string.IsNullOrEmpty(query.MimeType))
            predicate = predicate.Add(x => x.Path.Contains(query.MimeType));
        return predicate;
    }
}