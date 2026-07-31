namespace Digital.Net.Core;

public static class CoreSettings
{
    public const long DefaultMaxAvatarSize = 2097152;
    public const long DefaultSessionIdleExpiration = 7200000;
    public const long DefaultSessionAbsoluteExpiration = 604800000;
    public const string DefaultFileSystemPath = "/digital_net_storage";
    public const int DefaultAuditRetentionDays = 90;
    public const int DefaultForwardLimit = 1;

    public const string ApplicationNameKey = "ApplicationName";
    public const string ApplicationDomainKey = "ApplicationDomain";
    public const string CorsAllowedOriginsKey = "CorsAllowedOrigins";
    public const string ConnectionStringKey = "Database:ConnectionString";
    public const string FileSystemPathKey = "FileSystemPath";
    public const string SessionIdleExpirationKey = "Auth:SessionIdleExpiration";
    public const string SessionAbsoluteExpirationKey = "Auth:SessionAbsoluteExpiration";
    public const string ApplicationKeyKey = "Auth:ApplicationKey";
    public const string AuditRetentionDaysKey = "Audit:RetentionDays";
    public const string ForwardedHeadersKnownProxiesKey = "ForwardedHeaders:KnownProxies";
    public const string ForwardedHeadersKnownIPNetworksKey = "ForwardedHeaders:KnownIPNetworks";
    public const string ForwardedHeadersForwardLimitKey = "ForwardedHeaders:ForwardLimit";
    public const string GitBranchKey = "Git:Branch";
    public const string GitTagKey = "Git:Tag";
    public const string GitCommitKey = "Git:Commit";
}