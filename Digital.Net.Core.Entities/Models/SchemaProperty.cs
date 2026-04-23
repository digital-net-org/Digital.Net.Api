using System.Reflection;
using Digital.Net.Core.Entities.Attributes;

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
    public string[]? EnumValues { get; }

    /// <summary>
    ///     Get a schema of the entity describing its properties.
    /// </summary>
    /// <returns>Schema of the entity</returns>
    public static List<SchemaProperty<T>> Get() =>
        typeof(T).GetProperties().Select(property => new SchemaProperty<T>(property)).ToList();
}
