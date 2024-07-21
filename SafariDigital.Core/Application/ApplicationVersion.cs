using System.Reflection;

namespace SafariDigital.Core.Application;

public static class ApplicationVersion
{
    private static Assembly GetEntryAssembly() => Assembly.GetEntryAssembly();

    public static string GetAssemblyVersion() =>
        GetEntryAssembly().GetName().Version?.ToString() ?? "Not set";
}