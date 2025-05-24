using Microsoft.Extensions.Configuration;

namespace Digital.Net.Api.Services.Application;

public class ApplicationService(
    IConfiguration configuration
) : IApplicationService
{
    public ApplicationVersion GetVersion() =>
        new()
        {
            /*
             * NOTE: These variables are sets in the Dockerfile. Do not change them without updating
             * the Dockerfile.
             */
            Name = configuration.GetApplicationName(),
            Version = configuration["git_version"] ?? string.Empty,
            CommitSha = configuration["git_commit_sha"] ?? string.Empty,
            Release = configuration["git_release"] ?? string.Empty,
        };
}