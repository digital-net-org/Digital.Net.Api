using System.Collections;
using System.Reflection;
using Digital.Net.Api.Core.Extensions.StringUtilities;
using Digital.Net.Api.Entities.Exceptions;
using Digital.Net.Api.Entities.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace Digital.Net.Api.Entities.Services;

public class EntityValidator<TContext>(
    TContext context
) : IEntityValidator<TContext> where TContext : DbContext
{
    public List<SchemaProperty<T>> GetSchema<T>() where T : Entity =>
        typeof(T).GetProperties().Select(property => new SchemaProperty<T>(property)).ToList();

    private void ValidateDynamicPayload(Entity entity)
    {
        var schema = typeof(EntityValidator<TContext>)
            .GetMethod("GetSchema")!
            .MakeGenericMethod(entity.GetType())
            .Invoke(this, null);

        foreach (var property in entity.GetType().GetProperties())
        {
            var value = property.GetValue(entity);
            var schemaProp = ((IEnumerable<object>)schema!)
                .FirstOrDefault(p => 
                    (string)p.GetType().GetProperty("Name")!.GetValue(p)! == property.Name);

            var validateMethod = typeof(EntityValidator<TContext>)
                .GetMethod("ValidateProperty")!
                .MakeGenericMethod(entity.GetType());

            validateMethod.Invoke(this, [value, property.Name, schemaProp]);
        }
    }

    private Type? GetNestedType<T>(string path)
    {
        var pathParts = path.ExtractFromPath();
        var nestedKey = pathParts.FirstOrDefault();
        if (nestedKey == null)
            return null;
        var property = typeof(T).GetProperty(
            nestedKey, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase
        );
        if (property == null)
            return null;

        var type = property.PropertyType;
        if (type != typeof(string) && typeof(IEnumerable).IsAssignableFrom(type))
            return type.IsGenericType
                ? type.GetGenericArguments().First()
                : type.HasElementType
                    ? type.GetElementType()
                    : null;
        return type;
    }

    public void ValidateCreatePayload<T>(T entity) where T : Entity
    {
        var schema = GetSchema<T>();
        foreach (var property in entity.GetType().GetProperties())
            ValidateProperty(
                property.GetValue(entity),
                property.Name,
                schema.FirstOrDefault(x => x.Name == property.Name)
            );
    }

    public void ValidatePatchPayload<T>(JsonPatchDocument<T> patch) where T : Entity
    {
        var schema = GetSchema<T>();
        foreach (var o in patch.Operations)
        {
            if (o.value is null || (o.op != "add" && o.op != "replace"))
                return;

            var propertyType = GetNestedType<T>(o.path);
            if (typeof(Entity).IsAssignableFrom(propertyType))
            {
                var entity = JObject.FromObject(o.value!).ToObject(propertyType) as Entity
                             ?? throw new EntityValidationException($"{o.path}: Invalid entity type.");
                ValidateDynamicPayload(entity);
                return;
            }

            var schemaProperty = schema
                .FirstOrDefault(x =>
                    x.Name.Equals(o.path.ExtractFromPath().First(), StringComparison.CurrentCultureIgnoreCase));

            if (schemaProperty is null)
                return;

            ValidateProperty(o.value, o.path, schemaProperty);
            if (schemaProperty.IsIdentity || schemaProperty.IsReadOnly)
                throw new EntityValidationException($"{o.path}: This field is read-only.");
        }
    }

    public void ValidateProperty<T>(object? value, string path, SchemaProperty<T>? property) where T : Entity
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

        if (property.IsIdentity)
            throw new EntityValidationException($"{path}: This field is read-only.");

        if (property.IsRequired && value is null)
            throw new EntityValidationException($"{path}: This field is required and cannot be null.");

        if (property is { IsRequired: true, Type: "String" } && string.IsNullOrWhiteSpace(value.ToString()))
            throw new EntityValidationException($"{path}: This field is required and cannot be empty.");

        if (property is { MaxLength: > 0, Type: "String" } && value.ToString()?.Length > property.MaxLength)
            throw new EntityValidationException($"{path}: Maximum length exceeded.");

        if (property.IsUnique && context.Set<T>().Any(x => EF.Property<object>(x, property.Name).Equals(value)))
            throw new EntityValidationException($"{path}: This value violates a unique constraint.");

        if (property.RegexValidation is not null && !property.RegexValidation.IsMatch(value.ToString() ?? ""))
            throw new EntityValidationException($"{path}: This value does not meet the requirements.");
    }
}