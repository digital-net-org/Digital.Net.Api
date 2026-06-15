using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using Digital.Net.Core.Entities.Attributes;
using Digital.Net.Core.Entities.Exceptions;

namespace Digital.Net.Core.Entities.Models;

/// <summary>
///     Schema describing an entity property.
/// </summary>
/// <typeparam name="T">The model of the entity</typeparam>
public class SchemaProperty<T>
    where T : class, IEntity
{
    private static readonly IReadOnlyList<SchemaProperty<T>> Cached =
        typeof(T).GetProperties().Select(property => new SchemaProperty<T>(property)).ToArray();

    private readonly Func<T, object?> _getter;
    private readonly Regex? _regex;

    public SchemaProperty(PropertyInfo propertyInfo)
    {
        Name = propertyInfo.Name;
        Path = AttributeAnalyzer<T>.GetPath(propertyInfo);
        IsReadOnly = AttributeAnalyzer<T>.IsReadOnly(propertyInfo);
        IsSecret = AttributeAnalyzer<T>.IsSecret(propertyInfo);
        IsRequired = AttributeAnalyzer<T>.IsRequired(propertyInfo);
        IsUnique = AttributeAnalyzer<T>.IsUnique(propertyInfo);
        IsTemplatable = AttributeAnalyzer<T>.IsTemplatable(propertyInfo);
        MaxLength = AttributeAnalyzer<T>.MaxLength(propertyInfo);
        IsIdentity = AttributeAnalyzer<T>.IsIdentity(propertyInfo);
        IsForeignKey = AttributeAnalyzer<T>.IsForeignKey(propertyInfo);
        _regex = AttributeAnalyzer<T>.GetRegex(propertyInfo);
        RegexValidation = _regex?.ToString();
        OneOfValues = AttributeAnalyzer<T>.GetOneOf(propertyInfo);
        _getter = BuildGetter(propertyInfo);

        var underlying = Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType;
        if (underlying.IsEnum)
        {
            Type = "Enum";
            EnumValues = Enum.GetNames(underlying);
        }
        else
        {
            Type = underlying.Name;
            EnumValues = null;
        }
    }

    public string Name { get; }
    public string Path { get; }
    public string Type { get; }
    public bool IsReadOnly { get; }
    public bool IsSecret { get; }
    public bool IsRequired { get; }
    public bool IsUnique { get; }
    public bool IsTemplatable { get; }
    public int? MaxLength { get; }
    public bool IsIdentity { get; }
    public bool IsForeignKey { get; }
    public string? RegexValidation { get; }
    public IReadOnlyList<string>? OneOfValues { get; }
    public string[]? EnumValues { get; }

    /// <summary>
    ///     Get a schema of the entity describing its properties. Built once per closed type and shared.
    /// </summary>
    public static IReadOnlyList<SchemaProperty<T>> Get() => Cached;

    /// <summary>
    ///     Validate an <see cref="Entity" /> payload for an EFCore creation.
    /// </summary>
    public static void Validate(T entity)
    {
        foreach (var property in Cached)
            property.ValidatePath(property._getter(entity), property.Name);
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
        if (_regex is not null && !_regex.IsMatch(value.ToString() ?? ""))
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

    // Compiled accessor (boxed) so Validate reads values without a per-call GetValue reflection hit.
    private static Func<T, object?> BuildGetter(PropertyInfo info)
    {
        if (!info.CanRead) return static _ => null;
        var param = Expression.Parameter(typeof(T), "e");
        var body = Expression.Convert(Expression.Property(param, info), typeof(object));
        return Expression.Lambda<Func<T, object?>>(body, param).Compile();
    }
}