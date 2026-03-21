using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;

namespace Digital.Net.Core.RateLimiter.Limiters;

public static class GlobalLimiter
{
    public const string Policy = "Default";
    public static Action<RateLimiterOptions> Options => options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        options.AddFixedWindowLimiter(Policy, opts =>
        {
            opts.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            opts.PermitLimit = 2000;
            opts.QueueLimit = 500;
            opts.Window = TimeSpan.FromSeconds(1);
        });
    };
}