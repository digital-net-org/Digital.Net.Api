using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using Digital.Net.Cms.Context;
using Digital.Net.Cms.Http.Dto;
using Digital.Net.Cms.Http.Services;
using Digital.Net.Cms.Models.Medias;
using Digital.Net.Cms.Services;
using Digital.Net.Core.Accessors;
using Digital.Net.Core.Entities.Context;
using Digital.Net.Core.Http.RateLimiters;
using Digital.Net.Core.Http.Services.Authentication.Filters;
using Digital.Net.Core.Http.Services.Crud;
using Digital.Net.Core.Http.Services.Documents;
using Digital.Net.Core.Http.Services.Pagination;
using Digital.Net.Core.Services.Documents;
using Digital.Net.Core.Services.Documents.Exceptions;
using Digital.Net.Lib.Files;
using Digital.Net.Lib.Messages;
using Digital.Net.Lib.Predicates;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Cms.Http.Endpoints;

public static class MediaEndpoints
{
    public static IEndpointRouteBuilder MapCmsMediaEndpoints(this IEndpointRouteBuilder app)
    {
        var userRoutes = app
            .MapGroup("cms/media")
            .WithTags("CMS.Media")
            .RequireRateLimiting(GlobalLimiter.Policy)
            .RequireAuthentication(AuthorizeType.Jwt | AuthorizeType.ApiKey);

        userRoutes.MapCrudSchema<CmsContext, Media>();

        userRoutes
            .MapGet("{id:guid}", GetMediaById)
            .WithSummary("GetById: Media")
            .WithDescription(
                "Retrieves a media by its ID. Cross-context join: " +
                "the Media (CmsContext) is loaded with its Variants, then their Documents are batch-loaded from DigitalContext."
            );

        userRoutes
            .MapGet("", GetMediaList)
            .WithSummary("GetPaginated: Media")
            .WithDescription(
                "Retrieves a paginated list of Media with their original Document metadata. " +
                "Variants are not loaded in the list view for performance — only on GetById."
            );

        userRoutes
            .MapGet("labels", GetMediaLabels)
            .WithSummary("GetMediaLabels")
            .WithDescription(
                "Returns the distinct, alphabetically-sorted list of labels currently in use across all " +
                "ArticleMedia and PageMedia pivots. Optional 'search' querystring narrows the result with a " +
                "case-insensitive LIKE filter."
            );

        userRoutes
            .MapGet("content-types", GetSupportedContentTypes)
            .WithSummary("GetContentTypes")
            .WithDescription("Returns the list of MIME types accepted by the media upload endpoint.");

        userRoutes
            .MapGet("max-size", GetMaxFileSize)
            .WithSummary("GetMaxFileSize")
            .WithDescription("Returns the maximum file size (in bytes) accepted by the media upload endpoint.");

        userRoutes
            .MapPost("", UploadMedia)
            .DisableAntiforgery()
            .WithSummary("Upload")
            .WithDescription("Uploads a new media image.");

        userRoutes.MapCrudPatch<CmsContext, Media>();
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

    private static async Task<Results<Ok<Result<List<string>>>, InternalServerError<Result<List<string>>>>>
        GetMediaLabels(
            [FromQuery]
            string? search,
            CancellationToken ct,
            MediaLabelService mediaLabelService
        )
    {
        var result = await mediaLabelService.GetExistingLabels(search, ct);
        return result.HasError
            ? TypedResults.InternalServerError(result)
            : TypedResults.Ok(result);
    }

    private static Ok<Result<IReadOnlyList<string>>> GetSupportedContentTypes() =>
        TypedResults.Ok(new Result<IReadOnlyList<string>>(MediaUploadConstraints.SupportedMimeTypes));

    private static Ok<Result<long>> GetMaxFileSize() =>
        TypedResults.Ok(new Result<long>(MediaUploadConstraints.MaxFileSize));

    private static async Task<IResult> UploadMedia(
        IFormFile file,
        [FromForm]
        string name,
        [FromForm]
        string? alt,
        IDocumentService documentService,
        CrudService<CmsContext, Media> crudService,
        IUserAccessor userContextService
    )
    {
        if (file.Length > MediaUploadConstraints.MaxFileSize)
            return Results.BadRequest(
                $"File exceeds the maximum size of {MediaUploadConstraints.MaxFileSize / (1024 * 1024)} MB.");

        if (!MediaUploadConstraints.SupportedMimeTypes.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase))
            return Results.BadRequest($"Unsupported file type '{file.ContentType}'.");

        var user = userContextService.GetUser();
        await using var stream = file.OpenReadStream();
        var definition = new FileDefinition
        {
            FileName = file.FileName,
            MimeType = file.ContentType,
            FileSize = file.Length
        };
        var docResult = await documentService.SaveDocumentAsync(stream, definition, user);
        if (docResult.HasError || docResult.Value is null)
            return Results.BadRequest(docResult);

        var media = new Media
        {
            Name = name,
            Alt = alt,
            DocumentId = docResult.Value.Id
        };
        var result = await crudService.Create(media);

        return result.HasError
            ? Results.BadRequest(result)
            : Results.Ok(result);
    }

    private static async Task<IResult> DeleteMedia(Guid id, MediaService mediaService)
    {
        var result = await mediaService.DeleteMediaAsync(id);
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
        [FromQuery]
        int? w,
        [FromQuery]
        int? q,
        CmsContext context,
        MediaService mediaService,
        DocumentCacheService documentCacheService,
        IUserAccessor userContextService
    )
    {
        var media = await context.Media.FindAsync(id);
        if (media is null)
            return Results.NotFound();

        // Unpublished media: 404 for Application auth, accessible for JWT/ApiKey
        if (!media.Published)
            try
            {
                userContextService.GetUser();
            }
            catch (UnauthorizedAccessException)
            {
                return Results.NotFound();
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

    private static async Task<Results<Ok<Result<MediaDto>>, NotFound<Result<MediaDto>>>> GetMediaById(
        Guid id,
        CmsContext cmsContext,
        DigitalContext digitalContext)
    {
        var result = new Result<MediaDto>();
        var media = await cmsContext.Media
            .Include(m => m.Variants)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);
        if (media is null)
            return TypedResults.NotFound(result);

        // Cross-context join: Document lives in DigitalContext, not in CmsContext.
        // We batch-load every Document referenced by the Media and its Variants in one query.
        var documentIds = new HashSet<Guid> { media.DocumentId };
        foreach (var variant in media.Variants)
            documentIds.Add(variant.DocumentId);

        var documents = await digitalContext.Documents
            .AsNoTracking()
            .Where(d => documentIds.Contains(d.Id))
            .ToDictionaryAsync(d => d.Id);

        if (!documents.TryGetValue(media.DocumentId, out var mediaDocument))
            return TypedResults.NotFound(result);

        result.Value = new MediaDto(media, mediaDocument, documents);
        return TypedResults.Ok(result);
    }

    private static async Task<Ok<QueryResult<MediaDto>>> GetMediaList(
        [AsParameters]
        MediaQuery query,
        CmsContext cmsContext,
        DigitalContext digitalContext)
    {
        query.ValidateParameters();
        var result = new QueryResult<MediaDto>();

        var predicate = BuildMediaPredicate(query);
        var items = cmsContext.Media.Where(predicate);
        var rowCount = await items.CountAsync();

        var config = new ParsingConfig { IsCaseSensitive = false };
        var orderBy = string.IsNullOrWhiteSpace(query.OrderBy) ? "CreatedAt" : query.OrderBy;
        var direction = string.Equals(query.Order, "desc", StringComparison.OrdinalIgnoreCase)
            ? " descending"
            : "";

        var entities = await items
            .AsNoTracking()
            .OrderBy(config, orderBy + direction)
            .Skip((query.Index - 1) * query.Size)
            .Take(query.Size)
            .ToListAsync();

        // Cross-context join (batch): one query for all root Documents of the page.
        // Variants are not loaded here — only on GetById.
        var documentIds = entities.Select(m => m.DocumentId).Distinct().ToList();
        var documents = await digitalContext.Documents
            .AsNoTracking()
            .Where(d => documentIds.Contains(d.Id))
            .ToDictionaryAsync(d => d.Id);

        result.Value = entities
            .Where(m => documents.ContainsKey(m.DocumentId))
            .Select(m => new MediaDto(m, documents[m.DocumentId]))
            .ToList();
        result.Total = rowCount;
        result.Index = query.Index;
        result.Size = query.Size;

        return TypedResults.Ok(result);
    }

    private static Expression<Func<Media, bool>> BuildMediaPredicate(MediaQuery query)
    {
        var predicate = PredicateBuilder.New<Media>();
        if (query.CreatedFrom.HasValue)
            predicate = predicate.Add(x => x.CreatedAt >= query.CreatedFrom);
        if (query.UpdatedFrom.HasValue)
            predicate = predicate.Add(x => x.UpdatedAt >= query.UpdatedFrom);
        if (query.CreatedTo is not null)
            predicate = predicate.Add(x => x.CreatedAt <= query.CreatedTo);
        if (query.UpdatedTo is not null)
            predicate = predicate.Add(x => x.UpdatedAt <= query.UpdatedTo);
        if (!string.IsNullOrEmpty(query.Name))
            predicate = predicate.Add(x => x.Name.StartsWith(query.Name));
        if (query.Published.HasValue)
            predicate = predicate.Add(x => x.Published == query.Published);
        return predicate;
    }
}