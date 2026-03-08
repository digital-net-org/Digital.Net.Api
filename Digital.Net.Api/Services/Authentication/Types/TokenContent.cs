namespace Digital.Net.Api.Services.Authentication.Types;

public class TokenContent(Guid id, string userAgent)
{
    public Guid Id { get; init; } = id;
    public string UserAgent { get; init; } = userAgent;
}