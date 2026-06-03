namespace Digital.Net.Core.Entities.Attributes;

/// <summary>
///     Indicates that the property should be treated as a secret.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class SecretAttribute : Attribute;
