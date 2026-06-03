using Digital.Net.Lib.Http.Accessors;
using Digital.Net.Lib.Origin;
using Microsoft.Extensions.DependencyInjection;

namespace Digital.Net.Lib.Http;

public static class LibHttpInjector
{
    public static IServiceCollection AddDigitalNetLibHttp(this IServiceCollection services) =>
        services
            .AddHttpContextAccessor()
            .AddScoped<IOriginAccessor, HttpOriginAccessor>();
}