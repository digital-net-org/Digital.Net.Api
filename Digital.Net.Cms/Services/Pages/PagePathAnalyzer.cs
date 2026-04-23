using System.Text.RegularExpressions;

namespace Digital.Net.Cms.Services.Pages;

public static partial class PagePathAnalyzer
{
    [GeneratedRegex(@":[a-zA-Z_][a-zA-Z0-9_]*")]
    private static partial Regex DynamicSlugRegex();

    public static bool HasDynamicSlug(string? path) => !string.IsNullOrEmpty(path) && DynamicSlugRegex().IsMatch(path);
}
