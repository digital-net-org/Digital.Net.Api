namespace Digital.Net.Api.Entities.Attributes;

/// <summary>
///     Adds a flag to the property that is returned in the schema.
/// </summary>
/// <param name="flag"></param>
[AttributeUsage(AttributeTargets.Property)]
public class DataFlagAttribute(string flag) : Attribute
{
    public string Flag { get; } = flag;
}
