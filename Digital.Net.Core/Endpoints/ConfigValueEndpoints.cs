using System.Linq.Expressions;
using Digital.Net.Core.Endpoints.Dto;
using Digital.Net.Core.Entities;
using Digital.Net.Core.Entities.Context;
using Digital.Net.Core.Entities.Models.ConfigValues;
using Digital.Net.Core.RateLimiter.Limiters;
using Digital.Net.Core.Services.Authentication.Filters;
using Digital.Net.Core.Services.Crud;
using Digital.Net.Core.Services.Pagination.Extensions;
using Digital.Net.Lib.Predicates;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Core.Endpoints;

public static class ConfigValueEndpoints
{
    public static IEndpointRouteBuilder MapConfigValueEndpoints(this IEndpointRouteBuilder app)
    {
        var controller = app
            .MapGroup("admin/config-value")
            .WithTags("ConfigValue")
            .RequireRateLimiting(GlobalLimiter.Policy)
            .RequireAuthentication(AuthorizeType.Jwt | AuthorizeType.ApiKey)
            .RequireAdmin();

        controller.MapCrudSchema<DigitalContext, ConfigValue>("");
        controller.MapCrudGet<DigitalContext, ConfigValue, ConfigValueDto>("");
        controller.MapPaginationGet<DigitalContext, ConfigValue, ConfigValueDto, ConfigValueQuery>(
            "",
            PaginationFilter
        );
        controller.MapCrudPost<DigitalContext, ConfigValue, ConfigValuePayload>("");
        controller.MapCrudPatch<DigitalContext, ConfigValue>("");
        controller.MapCrudDelete<DigitalContext, ConfigValue>("");

        return app;
    }

    private static Expression<Func<ConfigValue, bool>> PaginationFilter(
        this Expression<Func<ConfigValue, bool>> predicate,
        ConfigValueQuery query
    )
    {
        if (!string.IsNullOrEmpty(query.Name))
            predicate = predicate.Add(x => EF.Functions.Like(x.Name, $"%{EFCoreUtils.EscapeLike(query.Name)}%"));
        if (query.Type.HasValue)
            predicate = predicate.Add(x => x.Type == query.Type.Value);
        return predicate;
    }
}