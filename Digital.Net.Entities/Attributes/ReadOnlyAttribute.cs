namespace Digital.Net.Entities.Attributes;

/// <summary>
///     Indicates that the property cannot be patched. Ignored on entity creation.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class ReadOnlyAttribute : Attribute { }
