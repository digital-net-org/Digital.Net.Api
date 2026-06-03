using Digital.Net.Cms.Context;
using Digital.Net.Cms.Models.Pages;
using Digital.Net.Cms.Services.Pages.Dto;
using Digital.Net.Cms.Services.Pages.Exceptions;
using Digital.Net.Core.Entities.Models;
using Digital.Net.Core.Services.Templating;
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

    public async Task<Result<PagePublicDto>> BuildPublicPage(PageBuildPayload payload, CancellationToken ct = default)
    {
        var result = new Result<PagePublicDto>();
        try
        {
            var (page, sources) = await ResolvePageAndSourcesAsync(payload, ct);
            if (sources is not null)
                TemplateInterpolator.HydrateInPlace(page, sources);

            var openGraph = await context.PageOpenGraphs
                .AsNoTracking()
                .Include(po => po.Child)
                .Where(po => po.ParentId == page.Id)
                .OrderBy(po => po.Order)
                .Select(po => po.Child)
                .ToListAsync(ct);

            if (sources is not null)
                foreach (var entry in openGraph)
                    TemplateInterpolator.HydrateInPlace(entry, sources);

            result.Value = new PagePublicDto(page)
            {
                OpenGraph = openGraph
                    .Select(e => new OpenGraphEntryPublicDto { Property = e.Property, Content = e.Content })
                    .ToList()
            };
        }
        catch (Exception ex)
        {
            result.AddError(ex);
        }

        return result;
    }

    public async Task<Result<(string contentType, string content)>> BuildPublicPageSheetResource(
        PageSheetBuildPayload payload,
        CancellationToken ct = default
    )
    {
        var result = new Result<(string contentType, string content)>();
        try
        {
            var (page, sources) = await ResolvePageAndSourcesAsync(payload, ct);
            var pageSheet = await context.PageSheets
                .AsNoTracking()
                .Include(ps => ps.Child)
                .FirstOrDefaultAsync(
                    ps => ps.ParentId == page.Id
                          && ps.ChildId == payload.SheetId
                          && ps.Child.Published,
                    ct
                ) ?? throw new InvalidPagePathException();

            var sheet = pageSheet.Child;
            if (sources is not null)
                TemplateInterpolator.HydrateInPlace(sheet, sources);

            var contentType = sheet.Type switch
            {
                "css" => "text/css",
                "js" => "application/javascript",
                "html" => "text/html",
                _ => "text/plain"
            };

            result.Value = (contentType, sheet.Content);
        }
        catch (Exception ex)
        {
            result.AddError(ex);
        }

        return result;
    }

    private async Task<(Page page, IReadOnlyDictionary<string, object>? sources)> ResolvePageAndSourcesAsync(
        PageBuildPayload payload,
        CancellationToken ct
    )
    {
        if (string.IsNullOrWhiteSpace(payload.Path))
            throw new InvalidPagePathException();

        var page = await context.Pages
            .AsNoTracking()
            .Where(p => p.Path == payload.Path && p.Published)
            .FirstOrDefaultAsync(ct) ?? throw new InvalidPagePathException();

        if (payload.PageType != page.EntityType)
            throw new InvalidPageTypeException();

        IReadOnlyDictionary<string, object>? sources = null;
        if (page.EntityType is not null && !string.IsNullOrEmpty(payload.PageSlug))
        {
            var source = await ResolveSourceAsync(page, payload.PageSlug, ct)
                         ?? throw new InvalidPagePathException();
            sources = new Dictionary<string, object>
            {
                [source.GetCanonicalType().Name.ToLowerInvariant()] = source
            };
        }

        return (page, sources);
    }

    private async Task<Entity?> ResolveSourceAsync(
        Page page,
        string slug,
        CancellationToken ct = default
    )
    {
        var result = page.EntityType switch
        {
            PageEntityType.Article => await context.Articles
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    a => a.Slug == slug
                         && a.PublishedAt != null
                         && a.PageId == page.Id,
                    ct),
            _ => null
        };
        return result ?? throw new InvalidPagePathException();
    }
}
