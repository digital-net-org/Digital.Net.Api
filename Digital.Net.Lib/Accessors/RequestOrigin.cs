namespace Digital.Net.Lib.Accessors;

public sealed record RequestOrigin(string? IpAddress, string? UserAgent, string? ClientId = null);