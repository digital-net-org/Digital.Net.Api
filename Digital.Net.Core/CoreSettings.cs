namespace Digital.Net.Core;

public static class CoreSettings
{
    public const long DefaultMaxAvatarSize = 2097152;
    public const long DefaultAuthJwtRefreshExpiration = 3600000;
    public const long DefaultAuthJwtBearerExpiration = 300000;
    public const string DefaultFileSystemPath = "/digital_net_storage";
    public const int DefaultAuditRetentionDays = 90;

    public const string ApplicationNameKey = "ApplicationName";
    public const string DomainKey = "Domain";
    public const string CorsAllowedOriginsKey = "CorsAllowedOrigins";
    public const string ConnectionStringKey = "Database:ConnectionString";
    public const string FileSystemPathKey = "FileSystemPath";
    public const string JwtRefreshExpirationKey = "Auth:JwtRefreshExpiration";
    public const string JwtBearerExpirationKey = "Auth:JwtBearerExpiration";
    public const string JwtSecretKey = "Auth:JwtSecret";
    public const string ApplicationKeyKey = "Auth:ApplicationKey";
    public const string AuditRetentionDaysKey = "Audit:RetentionDays";
    public const string GitOriginKey = "Git:Origin";
    public const string GitCommitShaKey = "Git:CommitSha";
    public const string GitReleaseKey = "Git:Release";
}