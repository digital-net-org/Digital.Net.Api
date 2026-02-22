using System.Threading.Tasks;
using Digital.Net.Api.Sdk;
using Microsoft.AspNetCore.Builder;

namespace Digital.Net.Tests.Program;

public sealed class DigitalProgram
{
    private static async Task Main(string[] args)
    {
        var app = WebApplication.CreateBuilder(args)
            .AddDigitalSdk()
            .Build();

        app
            .UseDigitalSdk();

        await app.RunAsync();
    }
}