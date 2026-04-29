using System.Reflection;
using System.Text.Json;
using Digital.Net.Core.Entities.Attributes;
using Digital.Net.Core.Entities.Exceptions;
using Digital.Net.Core.Entities.Extensions;
using Digital.Net.Core.Entities.Models;
using Digital.Net.Lib.Messages;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Core.Entities.Pivots;

public class PivotPatchResolver<TContext, TParent, TChild, TPivot, TDto>(TContext context)
    : IPivotPatchResolver<TParent>
    where TContext : DbContext
    where TParent : Entity
    where TChild : Entity
    where TPivot : Pivot<TParent, TChild>, new()
    where TDto : class, IPivotPayload<TDto, TPivot, TChild>
{
    private static readonly PivotResolutionAttribute Resolution =
        typeof(TPivot).GetCustomAttribute<PivotResolutionAttribute>()
        ?? throw new InvalidOperationException(
            $"{typeof(TPivot).Name} must carry [PivotResolution(\"/path\", Ownership.…)] " +
            $"to be usable by {nameof(PivotPatchResolver<,,,,>)}."
        );

    public string VirtualPath => Resolution.VirtualPath;
    protected Ownership Mode => Resolution.Mode;

    public Result ValidateValue(JsonElement value, Guid parentId)
    {
        var result = new Result();
        if (value.ValueKind == JsonValueKind.Null)
            return result;
        if (value.ValueKind != JsonValueKind.Array)
            return result.AddError(new EntityValidationException($"{VirtualPath}: Value must be an array or null."));

        var items = PivotJson.Deserialize<TDto>(value);
        for (var i = 0; i < items.Count; i++)
        {
            try
            {
                var entity = items[i].ToChild();
                var schema = SchemaProperty<TChild>.Get();
                foreach (var property in entity.GetType().GetProperties())
                    schema
                        .FirstOrDefault(x => x.Name == property.Name)?
                        .ValidatePath(property.GetValue(entity), property.Name);
            }
            catch (EntityValidationException ex)
            {
                result.AddError(new EntityValidationException($"{VirtualPath}[{i}].{ex.Message}"));
            }
        }

        return result;
    }

    public virtual async Task ApplyAsync(JsonElement value, Guid parentId, CancellationToken ct)
    {
        var incoming = value.ValueKind == JsonValueKind.Array ? PivotJson.Deserialize<TDto>(value) : [];

        var existing = await context.Set<TPivot>()
            .Include(p => p.Child)
            .Where(p => p.ParentId == parentId)
            .ToListAsync(ct);

        var incomingIds = incoming
            .Select(d => d.Id)
            .Where(id => id is { } v && v != Guid.Empty)
            .Select(id => id!.Value)
            .ToHashSet();

        foreach (var pivot in existing.Where(p => !incomingIds.Contains(p.ChildId)))
            if (Mode == Ownership.Cascade) context.Remove(pivot.Child);
            else context.Set<TPivot>().Remove(pivot);

        for (var index = 0; index < incoming.Count; index++)
        {
            var dto = incoming[index];
            var id = dto.Id is { } v && v != Guid.Empty ? v : (Guid?)null;
            var match = id is not null ? existing.FirstOrDefault(p => p.ChildId == id) : null;

            if (match is null)
            {
                var child = dto.ToChild();
                context.Set<TChild>().Add(child);
                context.Set<TPivot>().Add(new TPivot { ParentId = parentId, ChildId = child.Id, Order = index });
            }
            else
            {
                dto.ApplyTo(match.Child);
                match.Order = index;
            }
        }

        context.MarkDirty<TParent>(parentId);
        await context.SaveEntityAsync(ct);
    }
}