using Digital.Net.Api.Core.Exceptions;
using Digital.Net.Api.Core.Extensions.StringUtilities;
using Digital.Net.Api.Core.Messages;
using Digital.Net.Api.Core.Models;
using Digital.Net.Api.Entities.Exceptions;
using Digital.Net.Api.Entities.Models;
using Digital.Net.Api.Entities.Repositories;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Api.Entities.Services;

public class EntityService<T, TContext>(IRepository<T, TContext> repository) : IEntityService<T, TContext>
    where T : Entity
    where TContext : DbContext
{
    public List<SchemaProperty<T>> GetSchema() =>
        typeof(T).GetProperties().Select(property => new SchemaProperty<T>(property)).ToList();

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
            foreach (var o in patch.Operations)
            {
                var key = o.path.ExtractFromPath().First();
                ValidatePatchPayload(
                    o.value,
                    o.path,
                    GetSchema().FirstOrDefault(x => x.Name.Equals(key, StringComparison.CurrentCultureIgnoreCase))
                );
            }

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

    public async Task<Result> Create(T entity)
    {
        var result = new Result();
        try
        {
            foreach (var property in entity.GetType().GetProperties())
                ValidatePayload(
                    property.GetValue(entity),
                    property.Name,
                    GetSchema().FirstOrDefault(x => x.Name == property.Name)
                );

            await OnCreate(entity);
            await repository.CreateAsync(entity);
            await repository.SaveAsync();
        }
        catch (Exception e)
        {
            result.AddError(e);
        }

        return result;
    }

    private void ValidatePatchPayload(
        object? value,
        string path,
        SchemaProperty<T>? property
    )
    {
        if (value is null || property is null)
            return;

        ValidatePayload(value, path, property);

        if (property.IsIdentity || property.IsReadOnly)
            throw new EntityValidationException($"{path}: This field is read-only.");
    }

    private void ValidatePayload(
        object? value,
        string path,
        SchemaProperty<T>? property
    )
    {
        if (value is null || property is null)
            return;

        if (
            (property.IsIdentity || property.IsForeignKey)
            && value.ToString() is "00000000-0000-0000-0000-000000000000" or "0"
        )
            return;

        if (path is "CreatedAt" or "UpdatedAt" && (DateTime)value == DateTime.MinValue)
            return;

        if (property.IsRequired && value is null)
            throw new EntityValidationException(
                $"{path}: This field is required and cannot be null."
            );

        if (
            property is { IsRequired: true, Type: "String" }
            && string.IsNullOrWhiteSpace(value.ToString())
        )
            throw new EntityValidationException(
                $"{path}: This field is required and cannot be empty."
            );

        if (property is { MaxLength: > 0, Type: "String" } && value.ToString()?.Length > property.MaxLength)
            throw new EntityValidationException($"{path}: Maximum length exceeded.");

        if (
            property.IsUnique
            && repository.Get(x => EF.Property<object>(x, property.Name).Equals(value)).Any()
        )
            throw new EntityValidationException(
                $"{path}: This value violates a unique constraint."
            );

        if (
            property.RegexValidation is not null
            && !property.RegexValidation.IsMatch(value.ToString() ?? "")
        )
            throw new EntityValidationException(
                $"{path}: This value does not meet the requirements."
            );
    }

    protected virtual Task OnCreate(T entity) => Task.CompletedTask;

    protected virtual Task OnPatch(T entity) => Task.CompletedTask;

    protected virtual Task OnDelete(T entity) => Task.CompletedTask;
}
