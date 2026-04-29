using System.Collections;
using System.Reflection;
using System.Text.RegularExpressions;
using Digital.Net.Core.Entities.Attributes;
using Digital.Net.Core.Entities.Exceptions;
using Digital.Net.Lib.String;
using Microsoft.AspNetCore.JsonPatch;
using Newtonsoft.Json.Linq;

namespace Digital.Net.Core.Entities.Models;

/// <summary>
///     Schema describing an entity property.
/// </summary>
/// <typeparam name="T">The model of the entity</typeparam>
public class SchemaProperty<T>
    where T : Entity
{
    public SchemaProperty(PropertyInfo propertyInfo)
    {
        Name = propertyInfo.Name;
        Path = AttributeAnalyzer<T>.GetPath(propertyInfo);
        DataFlag = propertyInfo.GetCustomAttribute<DataFlagAttribute>()?.Flag;
        IsReadOnly = AttributeAnalyzer<T>.IsReadOnly(propertyInfo);
        IsSecret = AttributeAnalyzer<T>.IsSecret(propertyInfo);
        IsRequired = AttributeAnalyzer<T>.IsRequired(propertyInfo);
        IsUnique = AttributeAnalyzer<T>.IsUnique(propertyInfo);
        MaxLength = AttributeAnalyzer<T>.MaxLength(propertyInfo);
        IsIdentity = AttributeAnalyzer<T>.IsIdentity(propertyInfo);
        IsForeignKey = AttributeAnalyzer<T>.IsForeignKey(propertyInfo);
        RegexValidation = AttributeAnalyzer<T>.GetRegex(propertyInfo)?.ToString();
        OneOfValues = AttributeAnalyzer<T>.GetOneOf(propertyInfo);

        var underlying = Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType;
        if (underlying.IsEnum)
        {
            Type = "Enum";
            EnumValues = Enum.GetNames(underlying);
        }
        else
        {
            Type = propertyInfo.PropertyType.Name;
            EnumValues = null;
        }
    }

    public string Name { get; }
    public string Path { get; }
    public string Type { get; }
    public string? DataFlag { get; }
    public bool IsReadOnly { get; }
    public bool IsSecret { get; }
    public bool IsRequired { get; }
    public bool IsUnique { get; }
    public int? MaxLength { get; }
    public bool IsIdentity { get; }
    public bool IsForeignKey { get; }
    public string? RegexValidation { get; }
    public IReadOnlyList<string>? OneOfValues { get; }
    public string[]? EnumValues { get; }

    /// <summary>
    ///     Get a schema of the entity describing its properties.
    /// </summary>
    public static List<SchemaProperty<T>> Get() =>
        typeof(T).GetProperties().Select(property => new SchemaProperty<T>(property)).ToList();

    /// <summary>
    ///     Validate an <see cref="Entity" /> payload for an EFCore creation.
    /// </summary>
    public static void Validate(T entity)
    {
        var schema = Get();
        foreach (var property in entity.GetType().GetProperties())
            schema.FirstOrDefault(x => x.Name == property.Name)?.ValidatePath(property.GetValue(entity), property.Name);
    }

    /// <summary>
    ///     Validate a JSON Patch document for an EFCore mutation. Each <c>add</c>/<c>replace</c>
    ///     op is dispatched, depth-2 paths targeting a navigation <see cref="Entity" /> are validated against that
    ///     entity's own schema. Other ops (<c>remove</c>/<c>test</c>/<c>copy</c>/<c>move</c>) and paths
    ///     deeper than two segments are skipped.
    /// </summary>
    public static void Validate(JsonPatchDocument<T> patch)
    {
        var schema = Get();
        foreach (var op in patch.Operations)
        {
            if (op.value is null || (op.op != "add" && op.op != "replace"))
                continue;

            var parts = op.path.ExtractFromPath();
            if (parts.Count is 0 or > 2)
                continue;

            var memberType = ResolveMemberType(parts[0]);
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
    ///     Validate a path for an EFCore creation.
    /// </summary>
    public void ValidatePath(object? value, string path)
    {
        if (value is null)
            return;
        if (
            (IsIdentity || IsForeignKey)
            && value.ToString() is "00000000-0000-0000-0000-000000000000" or "0"
        )
            return;
        if (path is "CreatedAt" or "UpdatedAt" && (DateTime)value == DateTime.MinValue)
            return;
        if (IsIdentity)
            throw new EntityValidationException($"{path}: This field is read-only.");
        if (IsRequired && Type == "String" && string.IsNullOrWhiteSpace(value.ToString()))
            throw new EntityValidationException($"{path}: This field is required and cannot be empty.");
        if (RegexValidation is not null && !Regex.IsMatch(value.ToString() ?? "", RegexValidation))
            throw new EntityValidationException($"{path}: This value does not meet the requirements.");
        if (OneOfValues is not null && !OneOfValues.Contains(value.ToString() ?? string.Empty))
            throw new EntityValidationException($"{path}: Value must be one of: {string.Join(", ", OneOfValues)}.");
    }

    /// <summary>
    ///     Validate a path for an EFCore mutation.
    /// </summary>
    public void ValidatePathMutation(object? value, string path)
    {
        ValidatePath(value, path);
        if (IsIdentity || IsReadOnly)
            throw new EntityValidationException($"{path}: This field is read-only.");
    }

    /// <summary>
    ///     Resolve the CLR type of the first path segment: returns the property type, or its
    ///     element type if it is a collection.
    /// </summary>
    private static Type? ResolveMemberType(string memberName)
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
                .GetMethod(nameof(Validate), flags, null, [actualType], null)
            ?? throw new InvalidOperationException(
                $"SchemaProperty<{actualType.Name}>.{nameof(Validate)}({actualType.Name}) not found."
            );

        validate.Invoke(null, BindingFlags.DoNotWrapExceptions, null, [entity], null);
    }
}
