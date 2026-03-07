using System.Linq.Expressions;
using Digital.Net.Core.Exceptions.types;
using Digital.Net.Core.Messages;
using Digital.Net.Core.Models;
using Digital.Net.Entities.Models;
using Digital.Net.Entities.Repositories;
using Microsoft.AspNetCore.JsonPatch;

namespace Digital.Net.Entities.Crud;

public class CrudService<T>(
    IRepository<T> repository,
    ICrudValidationService crudValidationService
) : ICrudService<T>
    where T : Entity
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
            crudValidationService.ValidatePatchPayload(patch);
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
            crudValidationService.ValidateCreatePayload(entity);
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
