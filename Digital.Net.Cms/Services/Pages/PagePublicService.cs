using Digital.Net.Cms.Context;
using Digital.Net.Cms.Models.Pages;
using Digital.Net.Cms.Services.Pages.Dto;
using Digital.Net.Cms.Services.Pages.Exceptions;
using Digital.Net.Core.Entities;
using Digital.Net.Core.Entities.Models;
using Digital.Net.Core.Entities.Templating;
using Digital.Net.Lib.Exceptions.types;
using Digital.Net.Lib.Messages;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Cms.Services.Pages;

public class PagePublicService(
    CmsContext context
)
{
    public async Task<Result<List<PageSheetInfoDto>>> GetPageSheetInfos(Guid id)
    {
        var result = new Result<List<PageSheetInfoDto>>();
        try
        {
            var pageExists = await context.Pages.AnyAsync(p => p.Id == id);
            if (!pageExists)
                throw new ResourceNotFoundException();

            result.Value = await context.PageSheets
                .AsNoTracking()
                .Include(ps => ps.Child)
                .Where(ps => ps.ParentId == id && ps.Child.Published == true)
                .OrderBy(ps => ps.Order)
                .Select(ps => new PageSheetInfoDto
                {
                    Id = ps.ChildId,
                    Name = ps.Child.Name,
                    Type = ps.Child.Type
                })
                .ToListAsync();
        }
        catch (Exception ex)
        {
            result.AddError(ex);
        }

        return result;
    }

    public async Task<Result<(string contentType, string content)>> GetPageSheetResource(Guid id, Guid sheetId)
    {
        var result = new Result<(string contentType, string content)>();
        try
        {
            var pageSheet = await context.PageSheets
                .AsNoTracking()
                .Include(ps => ps.Child)
                .FirstOrDefaultAsync(ps => ps.ParentId == id && ps.ChildId == sheetId);

            if (pageSheet is null || !pageSheet.Child.Published)
                throw new ResourceNotFoundException();

            var contentType = pageSheet.Child.Type switch
            {
                "css" => "text/css",
                "js" => "application/javascript",
                "html" => "text/html",
                _ => "text/plain"
            };

            result.Value = (contentType, pageSheet.Child.Content);
        }
        catch (Exception ex)
        {
            result.AddError(ex);
        }

        return result;
    }

    public async Task<Result<PagePublicDto>> BuildPublicPage(PageBuildPayload payload, CancellationToken ct = default)
    {
        var result = new Result<PagePublicDto>();
        try
        {
            if (string.IsNullOrWhiteSpace(payload.Path))
                throw new InvalidPagePathException();
            
            var page = await context.Pages
                .AsNoTracking()
                .Where(p => p.Path == payload.Path && p.Published)
                .FirstOrDefaultAsync(ct);

            if (page is null) throw new InvalidPagePathException();
            if (payload.PageType != page.EntityType) throw new InvalidPageTypeException();
            if (page.EntityType is not null && !string.IsNullOrEmpty(payload.PageSlug))
            {
                var source = await ResolveSourceAsync(page.EntityType.Value, payload.PageSlug, ct) ??
                             throw new InvalidPagePathException();
                TemplateInterpolator.HydrateInPlace(
                    page,
                    new Dictionary<string, object>
                    {
                        [EfCoreUtils.GetCanonicalType(source).Name.ToLowerInvariant()] = source
                    });
            }

            result.Value = new PagePublicDto(page);
        }
        catch (Exception ex)
        {
            result.AddError(ex);
        }

        return result;
    }

    private async Task<Entity?> ResolveSourceAsync(
        PageEntityType entityType,
        string slug,
        CancellationToken ct = default
    )
    {
        var result = entityType switch
        {
            PageEntityType.Article => await context.Articles
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Slug == slug && a.PublishedAt != null, ct),
            _ => null
        };
        return result ?? throw new InvalidPagePathException();
    }

    private static IReadOnlyDictionary<string, object> BuildSources(Entity source) =>
        new Dictionary<string, object>
        {
            [EfCoreUtils.GetCanonicalType(source).Name.ToLowerInvariant()] = source
        };
}