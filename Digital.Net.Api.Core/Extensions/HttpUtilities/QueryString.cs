namespace Digital.Net.Api.Core.Extensions.HttpUtilities;

public static class QueryString
{
    /// <summary>
    ///     Transform a query object into a query string.
    /// </summary>
    /// <param name="query">The query object to transform.</param>
    /// <returns>The query string.</returns>
    public static string ToQueryString(this object query)
    {
        var properties = query.GetType().GetProperties();
        var queryString = string.Empty;

        foreach (var property in properties)
        {
            var value = property.GetValue(query);
            if (value == null)
                continue;

            if (queryString.Length > 0)
                queryString += "&";

            queryString += $"{property.Name}={value}";
        }
        return string.IsNullOrEmpty(queryString) ? string.Empty : $"?{queryString}";
    }
}
