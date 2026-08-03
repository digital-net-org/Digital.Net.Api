using System.Text.RegularExpressions;

namespace Digital.Net.Cms.Http.Services;

public static partial class PagePathAnalyzer
{
    [GeneratedRegex(@":[a-zA-Z_][a-zA-Z0-9_]*")]
    private static partial Regex DynamicSlugRegex();
    public static bool HasDynamicSlug(string? path) => !string.IsNullOrEmpty(path) && DynamicSlugRegex().IsMatch(path);
    public static string ResolveDynamicPath(string pattern, string value) => DynamicSlugRegex().Replace(pattern, value);

    public static int CountDynamicSlugs(string? path) =>
        string.IsNullOrEmpty(path) ? 0 : DynamicSlugRegex().Count(path);

    /// <summary>
    ///     Returns true when the pattern covers the given path: same segment count, a dynamic segment
    ///     covers any non-empty segment, static segments must match exactly.
    /// </summary>
    public static bool IsPatternMatch(string? pattern, string? path)
    {
        if (string.IsNullOrEmpty(pattern) || string.IsNullOrEmpty(path))
            return false;

        var patternSegments = pattern.Split('/');
        var pathSegments = path.Split('/');
        if (patternSegments.Length != pathSegments.Length)
            return false;

        for (var i = 0; i < patternSegments.Length; i++)
        {
            if (DynamicSlugRegex().IsMatch(patternSegments[i]))
            {
                if (pathSegments[i].Length == 0)
                    return false;
            }
            else if (patternSegments[i] != pathSegments[i])
            {
                return false;
            }
        }

        return true;
    }
}
