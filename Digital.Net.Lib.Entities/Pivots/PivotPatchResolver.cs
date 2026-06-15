using System.Reflection;
using System.Text.Json;
using Digital.Net.Lib.Entities.Attributes;
using Digital.Net.Lib.Entities.Exceptions;
using Digital.Net.Lib.Entities.Extensions;
using Digital.Net.Lib.Entities.Models;
using Digital.Net.Lib.Messages;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Lib.Entities.Pivots;

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

    private static readonly IReadOnlyList<(SchemaProperty<TPivot> Schema, PropertyInfo DtoProp)> PivotCustomSchema =
        SchemaProperty<TPivot>
            .Get()
            .Where(s => typeof(TPivot).GetProperty(s.Name)?.DeclaringType != typeof(Pivot<TParent, TChild>))
            .Select(s => (Schema: s, DtoProp: typeof(TDto).GetProperty(s.Name)))
            .Where(x => x.DtoProp is not null)
            .Select(x => (x.Schema, x.DtoProp!))
            .ToArray();

    protected Ownership Mode => Resolution.Mode;

    public string VirtualPath => Resolution.VirtualPath;

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
            var dto = items[i];
            var hasValidId = dto.Id is { } v && v != Guid.Empty;
            if (Mode == Ownership.Cascade || !hasValidId)
                try
                {
                    SchemaProperty<TChild>.Validate(dto.ToChild());
                }
                catch (EntityValidationException ex)
                {
                    result.AddError(new EntityValidationException($"{VirtualPath}[{i}].{ex.Message}"));
                }

            foreach (var (pivotSchema, dtoProp) in PivotCustomSchema)
                try
                {
                    pivotSchema.ValidatePath(dtoProp.GetValue(dto), pivotSchema.Name);
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
                if (Mode == Ownership.Dissociate && id is not null)
                {
                    var childExists = await context.Set<TChild>().AnyAsync(c => c.Id == id.Value, ct);
                    if (!childExists)
                        throw new EntityValidationException(
                            $"{VirtualPath}[{index}]: Child with id {id} does not exist."
                        );
                    var newPivot = new TPivot { ParentId = parentId, ChildId = id.Value, Order = index };
                    dto.ApplyToPivot(newPivot);
                    context.Set<TPivot>().Add(newPivot);
                }
                else
                {
                    var child = dto.ToChild();
                    context.Set<TChild>().Add(child);
                    var newPivot = new TPivot { ParentId = parentId, ChildId = child.Id, Order = index };
                    dto.ApplyToPivot(newPivot);
                    context.Set<TPivot>().Add(newPivot);
                }
            }
            else
            {
                if (Mode == Ownership.Cascade) dto.ApplyTo(match.Child);
                match.Order = index;
                dto.ApplyToPivot(match);
            }
        }

        context.MarkDirty<TParent>(parentId);
        await context.SaveEntityAsync(ct);
    }
}