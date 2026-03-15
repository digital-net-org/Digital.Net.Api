namespace Digital.Net.Core.Entities.Models;

public static class EntityTypeExtensions
{
    public static bool IsEntity(this Type type) => type.BaseType == typeof(Entity);
}
