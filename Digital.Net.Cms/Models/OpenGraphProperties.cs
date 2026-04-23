namespace Digital.Net.Cms.Models;

public class OpenGraphPropertySchema
{
    public required string Key { get; init; }
    public bool AllowMultiple { get; init; }
}

public static class OpenGraphProperties
{
    public static readonly IReadOnlyList<OpenGraphPropertySchema> Schema = BuildSchema();

    public static readonly IReadOnlyDictionary<string, OpenGraphPropertySchema> ByKey =
        Schema.ToDictionary(p => p.Key, StringComparer.Ordinal);

    private static List<OpenGraphPropertySchema> BuildSchema() =>
    [
        // Basic
        new() { Key = "og:title" },
        new() { Key = "og:type" },
        new() { Key = "og:url" },
        new() { Key = "og:description" },
        new() { Key = "og:determiner" },
        new() { Key = "og:site_name" },
        new() { Key = "og:locale" },
        new() { Key = "og:locale:alternate", AllowMultiple = true },

        // Image (structured)
        new() { Key = "og:image",            AllowMultiple = true },
        new() { Key = "og:image:url",        AllowMultiple = true },
        new() { Key = "og:image:secure_url", AllowMultiple = true },
        new() { Key = "og:image:type",       AllowMultiple = true },
        new() { Key = "og:image:width",      AllowMultiple = true },
        new() { Key = "og:image:height",     AllowMultiple = true },
        new() { Key = "og:image:alt",        AllowMultiple = true },

        // Video (structured)
        new() { Key = "og:video",            AllowMultiple = true },
        new() { Key = "og:video:url",        AllowMultiple = true },
        new() { Key = "og:video:secure_url", AllowMultiple = true },
        new() { Key = "og:video:type",       AllowMultiple = true },
        new() { Key = "og:video:width",      AllowMultiple = true },
        new() { Key = "og:video:height",     AllowMultiple = true },

        // Audio (structured)
        new() { Key = "og:audio",            AllowMultiple = true },
        new() { Key = "og:audio:secure_url", AllowMultiple = true },
        new() { Key = "og:audio:type",       AllowMultiple = true },

        // Video types (movie / episode / tv_show)
        new() { Key = "video:actor",        AllowMultiple = true },
        new() { Key = "video:actor:role",   AllowMultiple = true },
        new() { Key = "video:director",     AllowMultiple = true },
        new() { Key = "video:writer",       AllowMultiple = true },
        new() { Key = "video:duration" },
        new() { Key = "video:release_date" },
        new() { Key = "video:tag",          AllowMultiple = true },
        new() { Key = "video:series" },

        // Music types (song / album / playlist / radio_station)
        new() { Key = "music:duration" },
        new() { Key = "music:album",        AllowMultiple = true },
        new() { Key = "music:album:disc" },
        new() { Key = "music:album:track" },
        new() { Key = "music:musician",     AllowMultiple = true },
        new() { Key = "music:song",         AllowMultiple = true },
        new() { Key = "music:song:disc" },
        new() { Key = "music:song:track" },
        new() { Key = "music:release_date" },
        new() { Key = "music:creator",      AllowMultiple = true },

        // Article
        new() { Key = "article:published_time" },
        new() { Key = "article:modified_time" },
        new() { Key = "article:expiration_time" },
        new() { Key = "article:author",  AllowMultiple = true },
        new() { Key = "article:section" },
        new() { Key = "article:tag",     AllowMultiple = true },

        // Book
        new() { Key = "book:author",     AllowMultiple = true },
        new() { Key = "book:isbn" },
        new() { Key = "book:release_date" },
        new() { Key = "book:tag",        AllowMultiple = true },

        // Profile
        new() { Key = "profile:first_name" },
        new() { Key = "profile:last_name" },
        new() { Key = "profile:username" },
        new() { Key = "profile:gender" },
    ];
}
