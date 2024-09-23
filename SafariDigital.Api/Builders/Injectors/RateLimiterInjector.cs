using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using SafariDigital.Core.Application;

namespace SafariDigital.Api.Builders.Injectors;

public static class RateLimiterInjector
{
    public static WebApplicationBuilder AddRateLimiter(this WebApplicationBuilder builder)
    {
        builder.Services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddFixedWindowLimiter("Default", opts => builder.Configuration.SetDefaultPolicy(opts));
        });
        return builder;
    }

    private static void SetDefaultPolicy(this IConfiguration configuration, FixedWindowRateLimiterOptions options)
    {
        var maxRequestAllowed = configuration.GetSection<int>(EApplicationSetting.SecurityMaxRequestAllowed);
        options.PermitLimit = maxRequestAllowed;
        options.Window =
            TimeSpan.FromMilliseconds(
                configuration.GetSection<long>(EApplicationSetting.SecurityMaxRequestWindow));
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = maxRequestAllowed;
    }
}