using Digital.Net.Cms.Services.Pages;
using Digital.Net.Cms.Services.Pages.Dto;
using Digital.Net.Cms.Services.Pages.Exceptions;
using Digital.Net.Core.RateLimiter.Limiters;
using Digital.Net.Core.Services.Authentication.Filters;
using Digital.Net.Lib.Exceptions.types;
using Digital.Net.Lib.Messages;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Digital.Net.Cms.Endpoints;

public static class PagePublicEndpoints
{
    public static IEndpointRouteBuilder MapCmsPagePublicEndpoints(this IEndpointRouteBuilder app)
    {
        var publicController = app
            .MapGroup("cms/pages/public")
            .WithTags("CMS.Pages.Public")
            .RequireRateLimiting(GlobalLimiter.Policy)
            .RequireAuthentication(AuthorizeType.Application | AuthorizeType.Jwt | AuthorizeType.ApiKey);

        publicController
            .MapPost("build", BuildPublicPage)
            .WithSummary("Build")
            .WithDescription(
                "Builds a published page response for the templated Path declared by the client, " +
                "interpolating [Templatable] fields with the entity instance identified by " +
                "(PageType, PageSlug). For static pages, omit PageType/PageSlug."
            );

        publicController
            .MapGet("{id:guid}/sheets", GetPublicPageSheets)
            .WithSummary("GetPageSheets")
            .WithDescription("Retrieves every published sheet owned by the page, ordered by load order.");

        publicController
            .MapGet("{id:guid}/sheets/{sheetId:guid}", GetPublicPageSheetResource)
            .WithSummary("GetPageSheetResource")
            .WithDescription(
                "Serves the raw content of a published sheet scoped to its page, with the matching Content-Type."
            );

        return app;
    }

    private static async Task<Results<
            Ok<Result<PagePublicDto>>,
            BadRequest<Result<PagePublicDto>>,
            InternalServerError<Result<PagePublicDto>>
        >>
        BuildPublicPage(
            [FromBody]
            PageBuildPayload payload,
            PagePublicService pagePublicService,
            CancellationToken ct
        )
    {
        var result = await pagePublicService.BuildPublicPage(payload, ct);
        if (result.HasErrorOfType<InvalidPagePathException>() || result.HasErrorOfType<InvalidPageTypeException>())
            return TypedResults.BadRequest(result);
        if (result.HasError)
            return TypedResults.InternalServerError(result);

        return TypedResults.Ok(result);
    }

    private static async
        Task<Results<Ok<Result<List<PageSheetInfoDto>>>, InternalServerError<Result<List<PageSheetInfoDto>>>, NotFound>>
        GetPublicPageSheets(
            Guid id,
            PagePublicService pagePublicService
        )
    {
        var result = await pagePublicService.GetPageSheetInfos(id);
        if (result.HasErrorOfType<ResourceNotFoundException>())
            return TypedResults.NotFound();
        if (result.HasError)
            return TypedResults.InternalServerError(result);

        return TypedResults.Ok(result);
    }

    private static async Task<Results<ContentHttpResult, InternalServerError, NotFound>> GetPublicPageSheetResource(
        Guid id,
        Guid sheetId,
        PagePublicService pagePublicService
    )
    {
        var result = await pagePublicService.GetPageSheetResource(id, sheetId);
        if (result.HasErrorOfType<ResourceNotFoundException>())
            return TypedResults.NotFound();
        if (result.HasError)
            return TypedResults.InternalServerError();

        return TypedResults.Content(result.Value.content, result.Value.contentType);
    }
}