using System.Text.RegularExpressions;

namespace Digital.Net.Cms.Http.Services;

public static partial class PagePathAnalyzer
{
    [GeneratedRegex(@":[a-zA-Z_][a-zA-Z0-9_]*")]
    private static partial Regex DynamicSlugRegex();
    public static bool HasDynamicSlug(string? path) => !string.IsNullOrEmpty(path) && DynamicSlugRegex().IsMatch(path);
    public static string ResolveDynamicPath(string pattern, string value) => DynamicSlugRegex().Replace(pattern, value);
}
