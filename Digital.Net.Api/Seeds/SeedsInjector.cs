using Digital.Net.Core.Environment;
using Digital.Net.Entities.Seeds;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Digital.Net.Api.Seeds;

public static class SeedsInjector
{
    public static WebApplicationBuilder AddDataSeeds(this WebApplicationBuilder builder)
    {
        if (AspNetEnv.IsDevelopment)
            builder.Services.AddScoped<ISeed, DevelopmentSeed>();

        return builder.ApplyDataSeeds();
    }
}