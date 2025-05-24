using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Text.RegularExpressions;
using Digital.Net.Api.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Api.Entities.Attributes;

/// <summary>
///     EF Core Attribute analyzer.
/// </summary>
/// <typeparam name="T"></typeparam>
// ReSharper disable once InconsistentNaming
public static class AttributeAnalyzer<T>
    where T : Entity
{
    public static bool IsRequired(string propertyName) =>
        typeof(T).GetProperty(propertyName)?.GetCustomAttribute<RequiredAttribute>() is not null;

    public static bool IsRequired(PropertyInfo property) =>
        property.GetCustomAttribute<RequiredAttribute>() is not null;

    public static bool IsUnique(string propertyName) =>
        typeof(T)
            .GetCustomAttributes<IndexAttribute>()
            .Any(attr => attr.IsUnique && attr.PropertyNames.Contains(propertyName));

    public static bool IsUnique(PropertyInfo property) =>
        typeof(T)
            .GetCustomAttributes<IndexAttribute>()
            .Any(attr => attr.IsUnique && attr.PropertyNames.Contains(property.Name));

    public static int MaxLength(string propertyName) =>
        typeof(T).GetProperty(propertyName)?.GetCustomAttribute<MaxLengthAttribute>()?.Length ?? 0;

    public static int? MaxLength(PropertyInfo property) =>
        property.GetCustomAttribute<MaxLengthAttribute>()?.Length;

    public static bool IsIdentity(string propertyName) =>
        typeof(T)
            .GetProperty(propertyName)
            ?.GetCustomAttribute<DatabaseGeneratedAttribute>()
            ?.DatabaseGeneratedOption == DatabaseGeneratedOption.Identity;

    public static bool IsIdentity(PropertyInfo property) =>
        property.GetCustomAttribute<DatabaseGeneratedAttribute>()?.DatabaseGeneratedOption
        == DatabaseGeneratedOption.Identity;

    public static bool IsForeignKey(string propertyName) =>
        typeof(T).GetProperty(propertyName)?.GetCustomAttribute<ForeignKeyAttribute>() is not null;

    public static bool IsForeignKey(PropertyInfo property) =>
        property.GetCustomAttribute<ForeignKeyAttribute>() is not null;

    public static bool IsSecret(string propertyName) =>
        typeof(T).GetProperty(propertyName)?.GetCustomAttribute<SecretAttribute>() is not null;

    public static bool IsSecret(PropertyInfo property) =>
        property.GetCustomAttribute<SecretAttribute>() is not null;

    public static bool IsReadOnly(string propertyName) =>
        typeof(T).GetProperty(propertyName)?.GetCustomAttribute<ReadOnlyAttribute>() is not null;

    public static bool IsReadOnly(PropertyInfo property) =>
        property.GetCustomAttribute<ReadOnlyAttribute>() is not null;

    public static string GetPath(string propertyName) =>
        typeof(T).GetProperty(propertyName)?.GetCustomAttribute<ColumnAttribute>()?.Name
        ?? propertyName;

    public static string GetPath(PropertyInfo property) =>
        property.GetCustomAttribute<ColumnAttribute>()?.Name ?? property.Name;

    public static string? GetDataFlag(string propertyName) =>
        typeof(T).GetProperty(propertyName)?.GetCustomAttribute<DataFlagAttribute>()?.Flag;

    public static string? GetDataFlag(PropertyInfo property) =>
        property.GetCustomAttribute<DataFlagAttribute>()?.Flag;

    public static Regex? GetRegex(string propertyName) =>
        typeof(T).GetProperty(propertyName)?.GetCustomAttribute<RegexValidationAttribute>()?.Regex;

    public static Regex? GetRegex(PropertyInfo property) =>
        property.GetCustomAttribute<RegexValidationAttribute>()?.Regex;
}
