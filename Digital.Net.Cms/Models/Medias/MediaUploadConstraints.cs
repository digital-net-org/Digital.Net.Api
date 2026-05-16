namespace Digital.Net.Cms.Models.Medias;

public static class MediaUploadConstraints
{
    public const long MaxFileSize = 10 * 1024 * 1024; // 10 MB

    public static readonly IReadOnlyList<string> SupportedMimeTypes =
    [
        "image/jpeg",
        "image/png",
        "image/webp",
        "image/gif",
        "image/svg+xml"
    ];
}
