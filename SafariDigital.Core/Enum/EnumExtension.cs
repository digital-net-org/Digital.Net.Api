using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace SafariDigital.Core.Enum;

public static class EnumExtension
{
    public static string GetDisplayName(this System.Enum enumValue) =>
        enumValue
            .GetType()
            .GetMember(enumValue.ToString())
            .First()
            .GetCustomAttribute<DisplayAttribute>()?
            .Name ?? string.Empty;
}