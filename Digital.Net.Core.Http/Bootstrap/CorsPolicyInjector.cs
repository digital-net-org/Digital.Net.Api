using Digital.Net.Lib.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Digital.Net.Core.Http.Bootstrap;

public static class CorsPolicyInjector
{
    public static WebApplicationBuilder AddDefaultCorsPolicy(this WebApplicationBuilder builder)
    {
        var domain = builder.Configuration.GetOrThrow<string>(CoreSettings.DomainKey);
        var allowedOrigins = new List<string>
        {
            $"https://{domain}",
            $"https://*.{domain}"
        };

        allowedOrigins.AddRange(
            builder.Configuration.Get<string[]>(CoreSettings.CorsAllowedOriginsKey)
            ?? []
        );

        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policyBuilder =>
            {
                policyBuilder
                    .WithOrigins(allowedOrigins.ToArray())
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        return builder;
    }
}