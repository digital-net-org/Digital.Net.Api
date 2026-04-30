using System.Linq.Expressions;
using Digital.Net.Cms.Context;
using Digital.Net.Cms.Endpoints.Dto;
using Digital.Net.Cms.Endpoints.Events;
using Digital.Net.Cms.Models.Medias;
using Digital.Net.Cms.Services.Medias;
using Digital.Net.Core.Entities.Models.Events;
using Digital.Net.Core.RateLimiter.Limiters;
using Digital.Net.Core.Services.Auditing;
using Digital.Net.Core.Services.Authentication;
using Digital.Net.Core.Services.Authentication.Filters;
using Digital.Net.Core.Services.Crud;
using Digital.Net.Core.Services.Documents;
using Digital.Net.Core.Services.Documents.Exceptions;
using Digital.Net.Core.Services.Pagination.Extensions;
using Digital.Net.Lib.Predicates;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Digital.Net.Cms.Endpoints;

public static class MediaEndpoints
{
    private const long MaxFileSize = 10 * 1024 * 1024; // 10 MB

    private static readonly string[] SupportedMimeTypes =
    [
        "image/jpeg", "image/png", "image/webp", "image/gif", "image/svg+xml"
    ];

    public static IEndpointRouteBuilder MapCmsMediaEndpoints(this IEndpointRouteBuilder app)
    {
        var userRoutes = app
            .MapGroup("cms/media")
            .WithTags("CMS.Media")
            .RequireRateLimiting(GlobalLimiter.Policy)
            .RequireAuthentication(AuthorizeType.Jwt | AuthorizeType.ApiKey);

        userRoutes.MapCrudGet<CmsContext, Media, MediaDto>();
        userRoutes.MapPaginationGet<CmsContext, Media, MediaDto, MediaQuery>(filter: PaginationFilter);

        userRoutes
            .MapPost("", UploadMedia)
            .DisableAntiforgery()
            .WithSummary("Upload")
            .WithDescription("Uploads a new media image.");

        userRoutes.MapCrudPatch<CmsContext, Media>(eventType: CmsEvents.UpdateMedia);
        userRoutes
            .MapDelete("{id:guid}", DeleteMedia)
            .WithSummary("Delete")
            .WithDescription("Deletes a media, its original document, and all cached variants.");

        userRoutes
            .MapDelete("{id:guid}/variants", PurgeMediaVariants)
            .WithSummary("PurgeMediaVariants")
            .WithDescription("Purges all cached variants for a given media.");

        userRoutes
            .MapDelete("variants/{variantId:guid}", PurgeVariant)
            .WithSummary("PurgeVariant")
            .WithDescription("Purges a specific variant by its ID.");

        userRoutes
            .MapDelete("variants", PurgeAllVariants)
            .WithSummary("PurgeAllVariants")
            .WithDescription("Purges all cached variants across all media.");

        app
            .MapGroup("cms/media")
            .WithTags("CMS.Media")
            .RequireRateLimiting(GlobalLimiter.Policy)
            .MapGet("image/{id:guid}.{ext}", GetMediaImage)
            .WithSummary("GetMediaImage")
            .WithDescription(
                "Serves a media image with optional on-demand resizing and compression. "
                + "Supports query parameters: w (width), q (quality 0-100, default 100)."
            )
            .RequireAuthentication(AuthorizeType.Application | AuthorizeType.Jwt | AuthorizeType.ApiKey);

        return app;
    }

    private static async Task<IResult> UploadMedia(
        IFormFile file,
        [FromForm] string name,
        [FromForm] string? alt,
        IDocumentService documentService,
        CrudService<CmsContext, Media> crudService,
        IAuditService auditService,
        IUserContextService userContextService
    )
    {
        if (file.Length > MaxFileSize)
            return Results.BadRequest("File exceeds the maximum size of 10 MB.");

        if (!SupportedMimeTypes.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase))
            return Results.BadRequest($"Unsupported file type '{file.ContentType}'.");

        var user = userContextService.GetUser();
        var docResult = await documentService.SaveDocumentAsync(file, user);
        if (docResult.HasError || docResult.Value is null)
            return Results.BadRequest(docResult);

        var media = new Media
        {
            Name = name,
            Alt = alt,
            DocumentId = docResult.Value.Id
        };
        var result = await crudService.Create(media);

        await auditService.RegisterEventAsync(
            CmsEvents.CreateMedia,
            result.HasError ? EventState.Failed : EventState.Success,
            result,
            userContextService.GetUserId()
        );

        return result.HasError
            ? Results.BadRequest(result)
            : Results.Ok(result);
    }
    
    private static async Task<IResult> DeleteMedia(
        Guid id,
        MediaService mediaService,
        IAuditService auditService,
        IUserContextService userContextService
    )
    {
        var result = await mediaService.DeleteMediaAsync(id);
        await auditService.RegisterEventAsync(
            CmsEvents.DeleteMedia,
            result.HasError ? EventState.Failed : EventState.Success,
            result,
            userContextService.GetUserId()
        );
        return result.HasError
            ? Results.NotFound(result)
            : Results.Ok(result);
    }

    private static async Task<IResult> PurgeMediaVariants(Guid id, MediaService mediaService) =>
        (await mediaService.PurgeMediaVariantsAsync(id)).HasError
            ? Results.NotFound()
            : Results.Ok();

    private static async Task<IResult> PurgeVariant(Guid variantId, MediaService mediaService) =>
        (await mediaService.PurgeVariantAsync(variantId)).HasError
            ? Results.NotFound()
            : Results.Ok();

    private static async Task<IResult> PurgeAllVariants(MediaService mediaService)
    {
        await mediaService.PurgeAllVariantsAsync();
        return Results.Ok();
    }

    private static async Task<IResult> GetMediaImage(
        Guid id,
        string ext,
        [FromQuery] int? w,
        [FromQuery] int? q,
        CmsContext context,
        MediaService mediaService,
        IDocumentCacheService documentCacheService,
        IUserContextService userContextService
    )
    {
        var media = await context.Media.FindAsync(id);
        if (media is null)
            return Results.NotFound();

        // Unpublished media: 404 for Application auth, accessible for JWT/ApiKey
        if (!media.Published)
        {
            try { userContextService.GetUser(); }
            catch (UnauthorizedAccessException) { return Results.NotFound(); }
        }

        var documentResult = await mediaService.GetOrCreateVariantAsync(media.Id, w, q);
        if (documentResult.HasError || documentResult.Value is null)
            return Results.NotFound();

        var cacheResult = documentCacheService.GetCachedDocumentFile(documentResult.Value);

        if (cacheResult.HasErrorOfType<DocumentNotFoundException>())
            return Results.NotFound();
        if (cacheResult.HasError)
            return Results.InternalServerError();
        if (cacheResult.Value is not FileContentResult fileContentResult)
            return Results.StatusCode(304);

        return Results.File(
            fileContentResult.FileContents,
            fileContentResult.ContentType
        );
    }

    private static Expression<Func<Media, bool>> PaginationFilter(
        Expression<Func<Media, bool>> predicate,
        MediaQuery query
    )
    {
        if (!string.IsNullOrEmpty(query.Name))
            predicate = predicate.Add(x => x.Name.StartsWith(query.Name));
        if (query.Published.HasValue)
            predicate = predicate.Add(x => x.Published == query.Published);
        return predicate;
    }
}
