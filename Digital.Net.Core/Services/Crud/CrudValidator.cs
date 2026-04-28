using System.Collections;
using System.Reflection;
using Digital.Net.Core.Entities.Exceptions;
using Digital.Net.Core.Entities.Models;
using Digital.Net.Lib.String;
using Microsoft.AspNetCore.JsonPatch;
using Newtonsoft.Json.Linq;

namespace Digital.Net.Core.Services.Crud;

public static class CrudValidator
{
    public static void ValidateCreatePayload<T>(T entity) where T : Entity
    {
        var schema = SchemaProperty<T>.Get();
        foreach (var property in entity.GetType().GetProperties())
            schema.FirstOrDefault(x => x.Name == property.Name)
                ?.Validate(property.GetValue(entity), property.Name);
    }

    public static void ValidatePatchPayload<T>(JsonPatchDocument<T> patch) where T : Entity
    {
        var schema = SchemaProperty<T>.Get();
        foreach (var o in patch.Operations)
        {
            if (o.value is null || (o.op != "add" && o.op != "replace"))
                return;

            var pathParts = o.path.ExtractFromPath();
            if (pathParts.Count > 2)
                return;

            var propertyType = GetNestedType<T>(pathParts);
            if (typeof(Entity).IsAssignableFrom(propertyType) && pathParts.Count == 2)
            {
                var parentSchemaProperty = schema.FirstOrDefault(x =>
                    x.Name.Equals(pathParts[0], StringComparison.CurrentCultureIgnoreCase));
                if (parentSchemaProperty is { IsReadOnly: true } or { IsIdentity: true })
                    throw new EntityValidationException($"{pathParts[0]}: This field is read-only.");
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

            schemaProperty.ValidateMutation(o.value, o.path);
        }
    }

    private static void ValidateDynamicPayload(Entity entity)
    {
        var schema = typeof(CrudValidator)
            .GetMethod(nameof(GetSchemaFor), BindingFlags.NonPublic | BindingFlags.Static)!
            .MakeGenericMethod(entity.GetType())
            .Invoke(null, null);

        foreach (var property in entity.GetType().GetProperties())
        {
            var value = property.GetValue(entity);
            var schemaProp = ((IEnumerable<object>)schema!)
                .FirstOrDefault(p =>
                    (string)p.GetType().GetProperty("Name")!.GetValue(p)! == property.Name);

            if (schemaProp is null) continue;
            schemaProp.GetType()
                .GetMethod("Validate")!
                .Invoke(schemaProp, BindingFlags.DoNotWrapExceptions, null, [value, property.Name], null);
        }
    }

    private static List<SchemaProperty<T>> GetSchemaFor<T>() where T : Entity => SchemaProperty<T>.Get();

    private static Type? GetNestedType<T>(List<string> path)
    {
        var nestedKey = path.FirstOrDefault();
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
}