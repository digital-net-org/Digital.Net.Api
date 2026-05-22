using Microsoft.Extensions.DependencyInjection;

namespace Digital.Net.Cms.Services.Medias;

public static class MediaInjector
{
    public static IServiceCollection AddMediaDependencies(this IServiceCollection services) =>
        services
            .AddScoped<MediaService>()
            .AddScoped<MediaLabelService>();
}