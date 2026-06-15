namespace Digital.Net.Lib.Entities.Attributes;

/// <summary>
///     On a DTO collection property, tells the auto-projector to order the projected child sequence by the named
///     property of the <b>child entity</b> before mapping. Translatable to SQL <c>ORDER BY</c> inside the projection.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class ProjectOrderByAttribute(string propertyName, bool descending = false) : Attribute
{
    public string PropertyName { get; } = propertyName;
    public bool Descending { get; } = descending;
}