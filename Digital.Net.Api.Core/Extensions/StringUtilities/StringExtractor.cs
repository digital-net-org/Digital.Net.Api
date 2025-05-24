namespace Digital.Net.Api.Core.Extensions.StringUtilities;

public static class StringExtractor
{
    /// <summary>
    ///     Extracts every parts of a path.
    /// </summary>
    /// <param name="path">A formatted path : "/Something/like/this" or "Like\That"</param>
    /// <returns></returns>
    public static List<string> ExtractFromPath(this string path) =>
        path.Split('/', '\\').Where(part => !string.IsNullOrEmpty(part)).ToList();
}