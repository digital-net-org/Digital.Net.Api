namespace Digital.Net.Cms.Http.Services;

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
        new() { Key = "og:locale:alternate" },

        // Image (structured)
        new() { Key = "og:image" },
        new() { Key = "og:image:url" },
        new() { Key = "og:image:secure_url" },
        new() { Key = "og:image:type" },
        new() { Key = "og:image:width" },
        new() { Key = "og:image:height" },
        new() { Key = "og:image:alt" },

        // Video (structured)
        new() { Key = "og:video" },
        new() { Key = "og:video:url" },
        new() { Key = "og:video:secure_url" },
        new() { Key = "og:video:type" },
        new() { Key = "og:video:width" },
        new() { Key = "og:video:height" },

        // Audio (structured)
        new() { Key = "og:audio" },
        new() { Key = "og:audio:secure_url" },
        new() { Key = "og:audio:type" },

        // Video types (movie / episode / tv_show)
        new() { Key = "video:actor" },
        new() { Key = "video:actor:role" },
        new() { Key = "video:director" },
        new() { Key = "video:writer" },
        new() { Key = "video:duration" },
        new() { Key = "video:release_date" },
        new() { Key = "video:tag" },
        new() { Key = "video:series" },

        // Music types (song / album / playlist / radio_station)
        new() { Key = "music:duration" },
        new() { Key = "music:album" },
        new() { Key = "music:album:disc" },
        new() { Key = "music:album:track" },
        new() { Key = "music:musician" },
        new() { Key = "music:song" },
        new() { Key = "music:song:disc" },
        new() { Key = "music:song:track" },
        new() { Key = "music:release_date" },
        new() { Key = "music:creator" },

        // Article
        new() { Key = "article:published_time" },
        new() { Key = "article:modified_time" },
        new() { Key = "article:expiration_time" },
        new() { Key = "article:author" },
        new() { Key = "article:section" },
        new() { Key = "article:tag" },

        // Book
        new() { Key = "book:author" },
        new() { Key = "book:isbn" },
        new() { Key = "book:release_date" },
        new() { Key = "book:tag" },

        // Profile
        new() { Key = "profile:first_name" },
        new() { Key = "profile:last_name" },
        new() { Key = "profile:username" },
        new() { Key = "profile:gender" },
    ];
}
