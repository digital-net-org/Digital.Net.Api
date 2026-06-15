namespace Digital.Net.Lib.Entities.Attributes;

/// <summary>
///     Marks a string property as an interpolation target (placeholders like {{ entity.field }} in its value will
///     be hydrated).
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class TemplatableAttribute : Attribute;
