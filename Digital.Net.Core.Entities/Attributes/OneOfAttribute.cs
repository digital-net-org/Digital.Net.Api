namespace Digital.Net.Core.Entities.Attributes;

/// <summary>
///     Restricts the allowed values for a string property to a finite set. Read by
///     <c>SchemaProperty&lt;T&gt;</c> and enforced by <c>SchemaProperty&lt;T&gt;.Validate</c>.
///     Use it for "string-enum" columns where the underlying CLR type is <see cref="string"/>.
///     For genuine CLR enums, the property type itself already conveys the allowed values.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class OneOfAttribute(params string[] values) : Attribute
{
    public IReadOnlyList<string> Values { get; } = values;
}
