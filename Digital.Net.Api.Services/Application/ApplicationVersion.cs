namespace Digital.Net.Api.Services.Application;

public class ApplicationVersion
{
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string CommitSha { get; set; } = string.Empty;
    public string Release { get; set; } = string.Empty;
}