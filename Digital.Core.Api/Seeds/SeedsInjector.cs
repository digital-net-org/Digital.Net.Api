using Digital.Lib.Net.Core.Environment;
using Digital.Lib.Net.Entities.Seeds;

namespace Digital.Core.Api.Seeds;

public static class SeedsInjector
{
    public static WebApplicationBuilder AddDataSeeds(this WebApplicationBuilder builder)
    {
        if (AspNetEnv.IsDevelopment)
            builder.Services.AddScoped<ISeed, DevelopmentSeed>();

        return builder.ApplyDataSeeds();
    }
}