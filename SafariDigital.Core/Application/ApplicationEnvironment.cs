namespace SafariDigital.Core.Application;

public static class ApplicationEnvironment
{
    public static string Get =>
        Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

    public static bool IsTest => Get == "Test";
}