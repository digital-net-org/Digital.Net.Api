using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using SafariDigital.Core.AppSettings;
using SafariLib.Core.Environment;

namespace SafariDigital.Api.Builders.Injectors;

public static class RateLimiterInjector
{
    public static WebApplicationBuilder AddRateLimiter(this WebApplicationBuilder builder)
    {
        builder.Services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddFixedWindowLimiter(
                "Default", opts => builder.Configuration.SetDefaultPolicy(opts)
            );
        });
        return builder;
    }

    private static void SetDefaultPolicy(this IConfiguration configuration, FixedWindowRateLimiterOptions options)
    {
        var maxRequestAllowed = configuration.GetSettingOrThrow<int>(EAppSetting.SecurityMaxRequestAllowed);
        options.PermitLimit = maxRequestAllowed;
        options.Window =
            TimeSpan.FromMilliseconds(configuration.GetSettingOrThrow<long>(EAppSetting.SecurityMaxRequestWindow));
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = maxRequestAllowed;
    }
}