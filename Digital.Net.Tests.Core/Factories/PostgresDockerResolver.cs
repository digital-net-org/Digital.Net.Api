using System;
using System.IO;
using System.Linq;

namespace Digital.Net.Tests.Core.Factories;

internal static class PostgresDockerResolver
{
    public static string? Resolve() => AutoDetect();

    private static string? AutoDetect()
    {
        if (OperatingSystem.IsWindows())
            return null; // Testcontainers picks up Docker Desktop named pipe on its own.

        var xdgRuntime = Environment.GetEnvironmentVariable("XDG_RUNTIME_DIR");
        var candidates = new[]
        {
            xdgRuntime is null ? null : Path.Combine(xdgRuntime, "podman", "podman.sock"),
            "/run/podman/podman.sock",
            "/var/run/docker.sock",
            xdgRuntime is null ? null : Path.Combine(xdgRuntime, "docker.sock")
        };

        return (
            from path in candidates
            where !string.IsNullOrEmpty(path)
            where File.Exists(path)
            select $"unix://{path}"
        ).FirstOrDefault();
    }
}