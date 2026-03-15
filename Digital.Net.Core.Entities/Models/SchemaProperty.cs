using System.Reflection;
using System.Text.RegularExpressions;
using Digital.Net.Core.Entities.Attributes;

namespace Digital.Net.Core.Entities.Models;

/// <summary>
///     Schema describing an entity property.
/// </summary>
/// <typeparam name="T">The model of the entity</typeparam>
public class SchemaProperty<T>(PropertyInfo propertyInfo)
    where T : Entity
{
    public string Name { get; } = propertyInfo.Name;
    public string Path { get; } = AttributeAnalyzer<T>.GetPath(propertyInfo);
    public string Type { get; } = propertyInfo.PropertyType.Name;
    public string? DataFlag { get; } = propertyInfo.GetCustomAttribute<DataFlagAttribute>()?.Flag;
    public bool IsReadOnly { get; } = AttributeAnalyzer<T>.IsReadOnly(propertyInfo);
    public bool IsSecret { get; } = AttributeAnalyzer<T>.IsSecret(propertyInfo);
    public bool IsRequired { get; } = AttributeAnalyzer<T>.IsRequired(propertyInfo);
    public bool IsUnique { get; } = AttributeAnalyzer<T>.IsUnique(propertyInfo);
    public int? MaxLength { get; } = AttributeAnalyzer<T>.MaxLength(propertyInfo);
    public bool IsIdentity { get; } = AttributeAnalyzer<T>.IsIdentity(propertyInfo);
    public bool IsForeignKey { get; } = AttributeAnalyzer<T>.IsForeignKey(propertyInfo);
    public Regex? RegexValidation { get; } = AttributeAnalyzer<T>.GetRegex(propertyInfo);

    /// <summary>
    ///     Get a schema of the entity describing its properties.
    /// </summary>
    /// <returns>Schema of the entity</returns>
    public static List<SchemaProperty<T>> Get() =>
        typeof(T).GetProperties().Select(property => new SchemaProperty<T>(property)).ToList();
}
