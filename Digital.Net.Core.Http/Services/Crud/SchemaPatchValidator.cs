using System.Collections;
using System.Reflection;
using Digital.Net.Core.Entities.Exceptions;
using Digital.Net.Core.Entities.Models;
using Digital.Net.Lib.String;
using Microsoft.AspNetCore.JsonPatch;
using Newtonsoft.Json.Linq;

namespace Digital.Net.Core.Http.Services.Crud;

/// <summary>
///     Validates a JSON Patch document against an entity's <see cref="SchemaProperty{T}" /> schema.
///     Lives in Core.Http because JSON Patch (RFC 6902) is an HTTP transport concern; the domain
///     schema (<see cref="SchemaProperty{T}" />) stays pure in Core.
/// </summary>
public static class SchemaPatchValidator
{
    /// <summary>
    ///     Validate a JSON Patch document for an EFCore mutation. Each <c>add</c>/<c>replace</c>
    ///     op is dispatched; depth-2 paths targeting a navigation <see cref="Entity" /> are validated against that
    ///     entity's own schema. Other ops (<c>remove</c>/<c>test</c>/<c>copy</c>/<c>move</c>) and paths
    ///     deeper than two segments are skipped.
    /// </summary>
    public static void Validate<T>(JsonPatchDocument<T> patch)
        where T : class, IEntity
    {
        var schema = SchemaProperty<T>.Get();
        foreach (var op in patch.Operations)
        {
            if (op.value is null || (op.op != "add" && op.op != "replace"))
                continue;

            var parts = op.path.ExtractFromPath();
            if (parts.Count is 0 or > 2)
                continue;

            var memberType = ResolveMemberType<T>(parts[0]);
            if (parts.Count == 2 && typeof(Entity).IsAssignableFrom(memberType))
            {
                var parent = schema.FirstOrDefault(x =>
                    x.Name.Equals(parts[0], StringComparison.OrdinalIgnoreCase));
                if (parent is { IsReadOnly: true } or { IsIdentity: true })
                    throw new EntityValidationException($"{parts[0]}: This field is read-only.");

                var nested = JObject.FromObject(op.value).ToObject(memberType) as Entity
                             ?? throw new EntityValidationException($"{op.path}: Invalid entity type.");
                ValidateRuntime(nested);
                continue;
            }

            schema.FirstOrDefault(x => x.Name.Equals(parts[0], StringComparison.OrdinalIgnoreCase))
                ?.ValidatePathMutation(op.value, op.path);
        }
    }

    /// <summary>
    ///     Resolve the CLR type of the first path segment: returns the property type, or its
    ///     element type if it is a collection.
    /// </summary>
    private static Type? ResolveMemberType<T>(string memberName)
        where T : class, IEntity
    {
        var property = typeof(T).GetProperty(
            memberName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        if (property is null)
            return null;
        var type = property.PropertyType;
        if (type == typeof(string) || !typeof(IEnumerable).IsAssignableFrom(type))
            return type;
        return type.IsGenericType
            ? type.GetGenericArguments()[0]
            : type.HasElementType
                ? type.GetElementType()
                : null;
    }

    /// <summary>
    ///     Dispatch to <c>SchemaProperty&lt;TActual&gt;.Validate(entity)</c> using the runtime
    ///     type of <paramref name="entity" />.
    /// </summary>
    private static void ValidateRuntime(Entity entity)
    {
        const BindingFlags flags = BindingFlags.Public | BindingFlags.Static;
        var actualType = entity.GetType();
        var validate =
            typeof(SchemaProperty<>)
                .MakeGenericType(actualType)
                .GetMethod("Validate", flags, null, [actualType], null)
            ?? throw new InvalidOperationException(
                $"SchemaProperty<{actualType.Name}>.Validate({actualType.Name}) not found."
            );

        validate.Invoke(null, BindingFlags.DoNotWrapExceptions, null, [entity], null);
    }
}
