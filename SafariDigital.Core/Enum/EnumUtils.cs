using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace SafariDigital.Core.Enum;

public static class EnumUtils
{
    public static string GetDisplayName(this System.Enum enumValue) =>
        enumValue
            .GetType()
            .GetMember(enumValue.ToString())
            .First()
            .GetCustomAttribute<DisplayAttribute>()?
            .Name ?? string.Empty;

    public static IEnumerable<string> GetEnumDisplayNames<T>() where T : System.Enum =>
        GetEnumValues<T>().Select(e => e.GetDisplayName());

    public static IEnumerable<T> GetEnumValues<T>() where T : System.Enum =>
        System.Enum.GetValues(typeof(T)).Cast<T>();
}