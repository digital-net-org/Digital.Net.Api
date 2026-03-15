using Digital.Net.Core.Entities.Seeds;
using Digital.Net.Lib.Environment;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Digital.Net.Core.Seeds;

public static class SeedsInjector
{
    public static WebApplicationBuilder AddDataSeeds(this WebApplicationBuilder builder)
    {
        if (AspNetEnv.IsDevelopment)
            builder.Services.AddScoped<ISeed, DevelopmentSeed>();

        return builder.ApplyDataSeeds();
    }
}