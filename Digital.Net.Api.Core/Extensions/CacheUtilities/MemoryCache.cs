using Microsoft.Extensions.Caching.Memory;

namespace Digital.Net.Api.Core.Extensions.CacheUtilities;

public static class MemoryCache
{
    public static T? TryGetValue<T>(this IMemoryCache memoryCache, string key)
    {
        memoryCache.TryGetValue(key, out T? value);
        return value;
    }
}