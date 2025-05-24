namespace Digital.Net.Api.Core.Settings;

public static class ApplicationSettingsAccessor
{
    public const string Domain = "Domain";
    public const string CorsAllowedOrigins = "CorsAllowedOrigins";
    public const string ConnectionString = "Database:ConnectionString";
    public const string UseSqlite = "Database:UseSqlite";

    public const string DefaultsFileSystemPath = "Defaults:FileSystemPath";
    public const string DefaultsJwtRefreshExpiration = "Defaults:Auth:JwtRefreshExpiration";
    public const string DefaultsJwtBearerExpiration = "Defaults:Auth:JwtBearerExpiration";
    public const string DefaultsJwtSecret = "Defaults:Auth:JwtSecret";
}