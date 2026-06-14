using System.Threading.RateLimiting;
using Digital.Net.Core.Http.Accessors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;

namespace Digital.Net.Core.Http.Security;

public static class RateLimiter
{
    public const string Policy = "Default";
    public const string UploadPolicy = "Upload";
    public const string ImagePolicy = "Image";

    public static Action<RateLimiterOptions> Options => options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

        options.AddPolicy(Policy, context => FixedWindowPerClient(context, 200, TimeSpan.FromSeconds(1)));
        options.AddPolicy(UploadPolicy, context => RateLimitPartition.GetConcurrencyLimiter(
            PartitionKey(context),
            _ => new ConcurrencyLimiterOptions { PermitLimit = 2, QueueLimit = 0 })
        );
        options.AddPolicy(ImagePolicy, context => FixedWindowPerClient(context, 60, TimeSpan.FromSeconds(1)));
    };

    private static string PartitionKey(HttpContext context) => context.GetRemoteIpAddress() ?? "unknown";

    private static RateLimitPartition<string> FixedWindowPerClient(HttpContext context, int permitLimit,
        TimeSpan window) =>
        RateLimitPartition.GetFixedWindowLimiter(PartitionKey(context), _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = permitLimit,
            Window = window,
            QueueLimit = 0,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst
        });
}
