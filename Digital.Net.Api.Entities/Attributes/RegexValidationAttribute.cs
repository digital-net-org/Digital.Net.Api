using System.Text.RegularExpressions;

namespace Digital.Net.Api.Entities.Attributes;

/// <summary>
///     This attributes triggers a validation on the property while patching the entity.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class RegexValidationAttribute(string regex) : Attribute
{
    public Regex Regex { get; } = new(regex);
}
