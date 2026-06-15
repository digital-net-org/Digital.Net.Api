using System.Text.Json;
using Digital.Net.Core.Entities.Extensions;
using Digital.Net.Core.Entities.Models;
using Digital.Net.Core.Entities.Pivots;
using Digital.Net.Core.Entities.Projection;
using Digital.Net.Lib.Exceptions.types;
using Digital.Net.Lib.Messages;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Core.Http.Services.Crud;

public class CrudService<TContext, T>(TContext context, PatchDispatcher<T> patchDispatcher)
    where TContext : DbContext
    where T : Entity
{
    /// <summary>
    ///     Loads every <typeparamref name="TChild" /> linked to <paramref name="parentId" /> through pivot table
    ///     <typeparamref name="TPivot" />, ordered by the pivot's <c>Order</c>, projecting each row straight to
    ///     <typeparamref name="TDto" /> in SQL (the pivot DTO flattens <c>pivot.Child</c>; see
    ///     <see cref="PivotProjector" />).
    /// </summary>
    public async Task<Result<List<TDto>>> GetChildren<TChild, TPivot, TDto>(
        Guid parentId,
        CancellationToken ct = default
    )
        where TChild : Entity
        where TPivot : Pivot<T, TChild>, new()
        where TDto : class
    {
        var result = new Result<List<TDto>>();
        var parentExists = await context.Set<T>().AsNoTracking().AnyAsync(p => p.Id == parentId, ct);
        if (!parentExists)
            return result.AddError(new ResourceNotFoundException());

        result.Value = await context.Set<TPivot>()
            .AsNoTracking()
            .Where(p => p.ParentId == parentId)
            .OrderBy(p => p.Order)
            .Select(PivotProjector.Project<TPivot, TChild, TDto>())
            .ToListAsync(ct);
        return result;
    }

    public async Task<Result> Patch(JsonElement patch, Guid id, CancellationToken ct = default)
    {
        var result = new Result();
        var entity = await context.Set<T>().FindAsync([id], ct);
        if (entity is null)
            return result.AddError(new ResourceNotFoundException());

        var (sanitized, childValidation) = patchDispatcher.ExtractAndValidate(patch, id);
        if (childValidation.HasError)
            return new Result().Merge(childValidation);

        var patchDocument = sanitized.GetPatchDocument<T>();
        var strategy = context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await context.Database.BeginTransactionAsync(ct);
            try
            {
                SchemaPatchValidator.Validate(patchDocument);
                patchDocument.ApplyTo(entity);
                // Always mark the root Modified. Needed for the mutation interceptor to emit a parent mutation.
                // Do not remove.
                context.Set<T>().Update(entity);
                await context.SaveEntityAsync(ct);

                await patchDispatcher.ApplyPendingAsync(id, ct);
                await tx.CommitAsync(ct);
                return result;
            }
            catch (Exception e)
            {
                await tx.RollbackAsync(ct);
                context.ChangeTracker.Clear();
                return result.AddError(e);
            }
        });
    }

    public async Task<Result> Delete(Guid id, CancellationToken ct = default)
    {
        var result = new Result();
        try
        {
            var entity = await context.Set<T>().FindAsync([id], ct);
            if (entity is null) throw new ResourceNotFoundException();
            context.Set<T>().Remove(entity);
            await context.SaveEntityAsync(ct);
        }
        catch (Exception e)
        {
            result.AddError(e);
        }

        return result;
    }

    public async Task<Result<Guid>> Create(T entity, CancellationToken ct = default)
    {
        var result = new Result<Guid>();
        try
        {
            SchemaProperty<T>.Validate(entity);
            await context.Set<T>().AddAsync(entity, ct);
            await context.SaveEntityAsync(ct);
            result.Value = entity.Id;
        }
        catch (Exception e)
        {
            result.AddError(e);
        }

        return result;
    }
}