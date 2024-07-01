using SafariDigital.Core.AppSettings;
using SafariLib.Core.Environment;
using SafariLib.Jwt;
using SafariLib.Jwt.Models;

namespace SafariDigital.Api.Builders.Injectors;

public static class JwtInjector
{
    public static IServiceCollection AddJwtService(this IServiceCollection services)
    {
        var config = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
        services.AddJwtService(new JwtOptions
        {
            Audience = config.GetSettingOrThrow<string>(EAppSetting.JwtAudience),
            BearerTokenExpiration = config.GetSettingOrThrow<long>(EAppSetting.JwtBearerExpiration),
            CookieName = config.GetSettingOrThrow<string>(EAppSetting.JwtCookieName),
            Issuer = config.GetSettingOrThrow<string>(EAppSetting.JwtIssuer),
            Secret = config.GetSettingOrThrow<string>(EAppSetting.JwtSecret),
            RefreshTokenExpiration = config.GetSettingOrThrow<long>(EAppSetting.JwtRefreshExpiration)
        });
        return services;
    }
}