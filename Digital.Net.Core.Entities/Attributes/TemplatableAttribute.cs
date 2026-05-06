namespace Digital.Net.Core.Entities.Attributes;

/// <summary>
///     Marks a string property as participating in the templating system:
///     either as an interpolation target (placeholders like {{ entity.field }}
///     in its value will be hydrated) or as an interpolation source (the field
///     can be referenced as a variable from another entity's templates).
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class TemplatableAttribute : Attribute;
