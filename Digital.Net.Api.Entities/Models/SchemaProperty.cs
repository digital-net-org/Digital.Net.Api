using System.Reflection;
using System.Text.RegularExpressions;
using Digital.Net.Api.Entities.Attributes;

namespace Digital.Net.Api.Entities.Models;

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
}
