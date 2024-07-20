using Microsoft.Extensions.Caching.Memory;

namespace SafariDigital.Core.Memory;

public static class MemoryCacheExtensions
{
    public static T? TryGetValue<T>(this IMemoryCache memoryCache, string key)
    {
        memoryCache.TryGetValue(key, out T? value);
        return value;
    }
}