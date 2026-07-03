namespace Digital.Net.Lib.Entities.Attributes;

/// <summary>
///     Marker for pagination sorting: only properties carrying this attribute can be used as a
///     client-supplied <c>orderBy</c> column.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class SortableAttribute : Attribute;