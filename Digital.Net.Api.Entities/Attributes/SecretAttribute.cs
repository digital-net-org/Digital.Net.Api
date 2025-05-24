namespace Digital.Net.Api.Entities.Attributes;

/// <summary>
///     Indicates that the property should be treated as a secret and should not be logged or serialized.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class SecretAttribute : Attribute;
