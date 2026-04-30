using System.Text.Json;
using Digital.Net.Cms.Context;
using Digital.Net.Cms.Endpoints.Events;
using Digital.Net.Cms.Models.Pages;
using Digital.Net.Cms.Services.Pages.Dto;
using Digital.Net.Core.Entities.Exceptions;
using Digital.Net.Core.Entities.Models.Events;
using Digital.Net.Core.Services.Auditing;
using Digital.Net.Core.Services.Crud;
using Digital.Net.Lib.Messages;

namespace Digital.Net.Cms.Services.Pages;

public class PageCrudService(
    CrudService<CmsContext, Page> crudService,
    CmsContext context,
    IAuditService auditService
)
{
    public async Task<Result<Guid>> CreatePage(PagePayload payload, Guid userId)
    {
        var result = new Result<Guid>();
        try
        {
            if (payload.EntityType is not null) ValidateEntityPath(payload.Path);
            result = await crudService.Create(new Page { Path = payload.Path, EntityType = payload.EntityType });
        }
        catch (Exception ex)
        {
            result.AddError(ex);
        }

        if (!result.HasError || result.HasErrorOfType<EntityValidationException>())
            await auditService.RegisterEventAsync(
                CmsEvents.CreatePage,
                result.HasError ? EventState.Failed : EventState.Success,
                result,
                userId
            );
            
        return result;
    }

    public async Task<Result> PatchPage(JsonElement patch, Guid pageId, Guid userId, CancellationToken ct = default)
    {
        var result = new Result();
        try
        {
            await ValidatePatch(patch, pageId);
            result = await crudService.Patch(patch, pageId, ct);
        }
        catch (Exception ex)
        {
            result.AddError(ex);
        }

        if (!result.HasError || result.HasErrorOfType<EntityValidationException>())
            await auditService.RegisterEventAsync(
                CmsEvents.UpdatePage,
                result.HasError ? EventState.Failed : EventState.Success,
                result,
                userId
            );

        return result;
    }

    private async Task ValidatePatch(JsonElement patch, Guid id)
    {
        if (patch.ValueKind != JsonValueKind.Array)
            return;

        string? entityTypeValue = null;
        string? entityPathValue = null;

        foreach (var op in patch.EnumerateArray())
        {
            if (!op.TryGetProperty("path", out var pathEl)) continue;
            var opPath = pathEl.GetString();
            if (opPath == "/EntityType" && op.TryGetProperty("value", out var valEl))
                entityTypeValue = valEl.GetString();
            else if (opPath == "/Path" && op.TryGetProperty("value", out var pathValEl))
                entityPathValue = pathValEl.GetString();
        }

        if (string.IsNullOrEmpty(entityTypeValue))
            return;
        entityPathValue ??= (await context.Set<Page>().FindAsync(id))?.Path;
        if (string.IsNullOrEmpty(entityPathValue))
            return;

        ValidateEntityPath(entityPathValue);
    }

    private void ValidateEntityPath(string path)
    {
        if (!PagePathAnalyzer.HasDynamicSlug(path))
            throw new EntityValidationException(
                "EntityType: This field requires at least one dynamic slug (:xxx) in the path."
            );
    }
}