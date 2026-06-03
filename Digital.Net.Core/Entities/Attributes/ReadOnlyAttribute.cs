namespace Digital.Net.Core.Entities.Attributes;

/// <summary>
///     Indicates that the property cannot be mutated. Ignored on entity creation.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class ReadOnlyAttribute : Attribute { }
