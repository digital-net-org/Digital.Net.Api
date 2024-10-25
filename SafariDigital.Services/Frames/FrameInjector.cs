using Microsoft.Extensions.DependencyInjection;

namespace SafariDigital.Services.Frames;

public static class FrameInjector
{
    public static IServiceCollection AddFrameService(this IServiceCollection services) =>
        services.AddScoped<IFrameService, FrameService>();
}