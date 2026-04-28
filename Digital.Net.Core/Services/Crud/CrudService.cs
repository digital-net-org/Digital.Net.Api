using System.Linq.Expressions;
using Digital.Net.Core.Entities.Models;
using Digital.Net.Lib.Exceptions.types;
using Digital.Net.Lib.Messages;
using Digital.Net.Lib.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Core.Services.Crud;

public class CrudService<TContext, T>(
    TContext context,
    CrudPatchDispatcher<T> patchDispatcher
) : ICrudService<T>
    where TContext : DbContext
    where T : Entity
{
    public Result<TModel> GetFirst<TModel>(Expression<Func<T, bool>> expression) where TModel : class
    {
        var result = new Result<TModel>();
        try
        {
            var value = context.Set<T>().FirstOrDefault(expression) ?? throw new ResourceNotFoundException();
            result.Value = Mapper.TryMap<T, TModel>(value);
            return result;
        }
        catch (Exception ex)
        {
            return result.AddError(ex);
        }
    }

    public Result<TModel> Get<TModel>(Guid id) where TModel : class
    {
        var result = new Result<TModel>();
        try
        {
            var entity = context.Set<T>().Find(id);
            if (entity is null)
                throw new ResourceNotFoundException();
            result.Value = Mapper.MapFromConstructor<T, TModel>(entity);
        }
        catch (Exception ex)
        {
            result.AddError(ex);
        }

        return result;
    }

    public async Task<Result<TModel>> GetHydratedAsync<TModel>(Guid id, CancellationToken ct = default)
        where TModel : class
    {
        var result = new Result<TModel>();
        var entity = await context.Set<T>().AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, ct);
        if (entity is null)
            return result.AddError(new ResourceNotFoundException());

        var dto = Mapper.MapFromConstructor<T, TModel>(entity);
        var pivots = await patchDispatcher.LoadAllAsync(id, ct);
        foreach (var (path, list) in pivots)
        {
            var prop = typeof(TModel).GetProperty(
                ToPropertyName(path),
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase
            );
            if (prop?.CanWrite is true) prop.SetValue(dto, list);
        }

        result.Value = dto;
        return result;
    }

    public async Task<Result> Patch(JsonPatchDocument<T> patch, Guid id)
    {
        var result = new Result();
        var entity = await context.Set<T>().FindAsync(id);
        if (entity is null)
            return result.AddError(new ResourceNotFoundException());
        try
        {
            CrudValidator.ValidatePatchPayload(patch);
            patch.ApplyTo(entity);
            context.Set<T>().Update(entity);

            await context.SaveAsync();
        }
        catch (Exception e)
        {
            result.AddError(e);
        }

        return result;
    }

    public async Task<Result> Delete(Guid id)
    {
        var result = new Result();
        var entity = await context.Set<T>().FindAsync(id);
        if (entity is null)
            return result.AddError(new ResourceNotFoundException());
        try
        {
            context.Set<T>().Remove(entity);
            await context.SaveAsync();
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
            CrudValidator.ValidateCreatePayload(entity);
            await context.Set<T>().AddAsync(entity);
            await context.SaveAsync();
            result.Value = entity.Id;
        }
        catch (Exception e)
        {
            result.AddError(e);
        }
        return result;
    }
}
