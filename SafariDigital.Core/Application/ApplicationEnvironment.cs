namespace SafariDigital.Core.Application;

public static class ApplicationEnvironment
{
    public static string GetEnvironment =>
        Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
        ?? throw new Exception("Environment variable ASPNETCORE_ENVIRONMENT is not set.");
}