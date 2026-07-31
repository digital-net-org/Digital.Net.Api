using Digital.Net.Lib.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Digital.Net.Core.Http.Bootstrap;

public static class CorsPolicyInjector
{
    /// <summary>
    ///     Restricts CORS to the explicitly declared origins. Nothing is inferred: an unlisted origin is
    ///     unreachable, and that list is what the CSRF header check ultimately rests on.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    ///     At least one origin must be declared in the configuration or this will fail.
    /// </exception>
    public static WebApplicationBuilder AddDefaultCorsPolicy(this WebApplicationBuilder builder)
    {
        var allowedOrigins = builder.Configuration.Get<string[]>(CoreSettings.CorsAllowedOriginsKey) ?? [];
        if (allowedOrigins.Length == 0)
            throw new InvalidOperationException(
                $"{CoreSettings.CorsAllowedOriginsKey} must declare at least one origin: requests carry "
                + "credentials, so no browser client can reach the API without it."
            );

        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policyBuilder =>
            {
                policyBuilder
                    .WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        return builder;
    }
}
