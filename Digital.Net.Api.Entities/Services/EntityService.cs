using System.Linq.Expressions;
using Digital.Net.Api.Core.Exceptions;
using Digital.Net.Api.Core.Messages;
using Digital.Net.Api.Core.Models;
using Digital.Net.Api.Entities.Models;
using Digital.Net.Api.Entities.Repositories;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Api.Entities.Services;

public class EntityService<T, TContext>(
    IRepository<T, TContext> repository,
    IEntityValidator<TContext> entityValidator
) : IEntityService<T, TContext>
    where T : Entity
    where TContext : DbContext
{
    public Result<TModel> GetFirst<TModel>(Expression<Func<T, bool>> expression) where TModel : class
    {
        var result = new Result<TModel>();
        try
        {
            var value = repository.Get(expression).FirstOrDefault() ?? throw new ResourceNotFoundException();
            result.Value = Mapper.TryMap<T, TModel>(value);
            return result;
        }
        catch (Exception ex)
        {
            return result.AddError(ex);
        }
    }

    public Result<TModel> Get<TModel>(Guid? id)
        where TModel : class => Get<TModel>(repository.GetById(id));

    public Result<TModel> Get<TModel>(int id)
        where TModel : class => Get<TModel>(repository.GetById(id));

    private static Result<TModel> Get<TModel>(T? entity)
        where TModel : class
    {
        var result = new Result<TModel>();
        if (entity is null)
            return result.AddError(new KeyNotFoundException("Entity not found."));
        result.Value = Mapper.MapFromConstructor<T, TModel>(entity);
        return result;
    }

    public async Task<Result> Patch(JsonPatchDocument<T> patch, Guid? id) =>
        await Patch(patch, await repository.GetByIdAsync(id));

    public async Task<Result> Patch(JsonPatchDocument<T> patch, int id) =>
        await Patch(patch, await repository.GetByIdAsync(id));

    private async Task<Result> Patch(JsonPatchDocument<T> patch, T? entity)
    {
        var result = new Result();
        if (entity is null)
            return result.AddError(new ResourceNotFoundException());
        try
        {
            entityValidator.ValidatePatchPayload(patch);
            patch.ApplyTo(entity);
            repository.Update(entity);
            await OnPatch(entity);
            await repository.SaveAsync();
        }
        catch (Exception e)
        {
            result.AddError(e);
        }

        return result;
    }

    public async Task<Result> Delete(Guid? id) => await Delete(await repository.GetByIdAsync(id));

    public async Task<Result> Delete(int id) => await Delete(await repository.GetByIdAsync(id));

    private async Task<Result> Delete(T? entity)
    {
        var result = new Result();
        if (entity is null)
            return result.AddError(new KeyNotFoundException("Entity not found."));
        try
        {
            await OnDelete(entity);
            repository.Delete(entity);
            await repository.SaveAsync();
        }
        catch (Exception e)
        {
            result.AddError(e);
        }

        return result;
    }

    public async Task<Result<string>> Create(T entity)
    {
        var result = new Result<string>();
        try
        {
            entityValidator.ValidateCreatePayload(entity);
            await OnCreate(entity);
            await repository.CreateAsync(entity);
            await repository.SaveAsync();
            result.Value = entity.GetType().GetProperty("Id")?.GetValue(entity)?.ToString();
        }
        catch (Exception e)
        {
            result.AddError(e);
        }

        return result;
    }

    protected virtual Task OnCreate(T entity) => Task.CompletedTask;

    protected virtual Task OnPatch(T entity) => Task.CompletedTask;

    protected virtual Task OnDelete(T entity) => Task.CompletedTask;
}
