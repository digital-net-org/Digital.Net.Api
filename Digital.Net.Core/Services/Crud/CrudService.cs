using System.Text.Json;
using Digital.Net.Core.Entities.Extensions;
using Digital.Net.Core.Entities.Models;
using Digital.Net.Core.Entities.Pivots;
using Digital.Net.Lib.Exceptions.types;
using Digital.Net.Lib.Formatters;
using Digital.Net.Lib.Messages;
using Digital.Net.Lib.Models;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Core.Services.Crud;

public class CrudService<TContext, T>(TContext context, PatchDispatcher<T> patchDispatcher)
    where TContext : DbContext
    where T : Entity
{
    /// <summary>
    ///     Get an entity by its primary key and map it to <typeparamref name="TModel"/> through
    ///     <typeparamref name="TModel"/>'s constructor — does NOT load any pivot child collection.
    ///     For child collections, call <see cref="GetChildren{TChild,TPivot,TDto}"/> separately.
    /// </summary>
    public async Task<Result<TModel>> Get<TModel>(Guid id, CancellationToken ct = default)
        where TModel : class
    {
        var result = new Result<TModel>();
        var entity = await context.Set<T>().AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, ct);
        if (entity is null)
            return result.AddError(new ResourceNotFoundException());
        result.Value = Mapper.MapFromConstructor<T, TModel>(entity);
        return result;
    }

    /// <summary>
    ///     Loads every <typeparamref name="TChild" /> linked to <paramref name="parentId" /> through
    ///     pivot table <typeparamref name="TPivot" /> and maps each row to <typeparamref name="TDto" /> the
    ///     corresponding constructor.
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

        var pivots = await context.Set<TPivot>()
            .AsNoTracking()
            .Include(p => p.Child)
            .Where(p => p.ParentId == parentId)
            .OrderBy(p => p.Order)
            .ToListAsync(ct);

        result.Value = pivots.Select(Mapper.TryMap<TPivot, TDto>).ToList();
        return result;
    }

    public async Task<Result> Patch(JsonElement patch, Guid id, CancellationToken ct = default)
    {
        var result = new Result();
        var entity = await context.Set<T>().FindAsync(id);
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
                SchemaProperty<T>.Validate(patchDocument);
                patchDocument.ApplyTo(entity);
                context.Set<T>().Update(entity);
                await context.SaveEntityAsync(ct);
                
                await patchDispatcher.ApplyPendingAsync(id, ct);
                await tx.CommitAsync(ct);
                return result;
            }
            catch (Exception e)
            {
                await tx.RollbackAsync(ct);
                return result.AddError(e);
            }
        });
    }

    public async Task<Result> Delete(Guid id)
    {
        var result = new Result();
        try
        {
            var entity = await context.Set<T>().FindAsync(id);
            if (entity is null) throw new ResourceNotFoundException();
            context.Set<T>().Remove(entity);
            await context.SaveEntityAsync();
        }
        catch (Exception e)
        {
            result.AddError(e);
        }

        return result;
    }

    public async Task<Result<Guid>> Create(T entity)
    {
        var result = new Result<Guid>();
        try
        {
            SchemaProperty<T>.Validate(entity);
            await context.Set<T>().AddAsync(entity);
            await context.SaveEntityAsync();
            result.Value = entity.Id;
        }
        catch (Exception e)
        {
            result.AddError(e);
        }
        return result;
    }
}
