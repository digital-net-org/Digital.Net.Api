using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;

namespace Digital.Net.Api.Sdk.RateLimiter.Limiters;

public static class GlobalLimiter
{
    public const string Policy = "Default";
    public static Action<RateLimiterOptions> Options => options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        options.AddFixedWindowLimiter(Policy, opts =>
        {
            opts.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            opts.PermitLimit = 100;
            opts.QueueLimit = 50;
            opts.Window = TimeSpan.FromMilliseconds(100);
        });
    };
}