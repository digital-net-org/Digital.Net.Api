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

    public Result<TModel> Get<TModel>(Guid id) where TModel : class
    {
        var result = new Result<TModel>();
        var entity = repository.GetById(id);
        if (entity is null)
            return result.AddError(new ResourceNotFoundException());
        result.Value = Mapper.MapFromConstructor<T, TModel>(entity);
        return result;
    }

    public async Task<Result> Patch(JsonPatchDocument<T> patch, Guid id)
    {
        var result = new Result();
        var entity = await repository.GetByIdAsync(id);
        if (entity is null)
            return result.AddError(new ResourceNotFoundException());
        try
        {
            entityValidator.ValidatePatchPayload(patch);
            patch.ApplyTo(entity);
            repository.Update(entity);
            await repository.SaveAsync();
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
        var entity = await repository.GetByIdAsync(id);
        if (entity is null)
            return result.AddError(new ResourceNotFoundException());
        try
        {
            repository.Delete(entity);
            await repository.SaveAsync();
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
            entityValidator.ValidateCreatePayload(entity);
            await repository.CreateAsync(entity);
            await repository.SaveAsync();
            result.Value = entity.Id;
        }
        catch (Exception e)
        {
            result.AddError(e);
        }
        return result;
    }
}
