using Digital.Net.Api.Sdk;

namespace Digital.Net.Api.App;

public sealed class Program
{
    private static async Task Main(string[] args)
    {
        var app = WebApplication.CreateBuilder(args)
            .AddDigitalSdk()
            .Build();

        await app
            .UseDigitalSdk()
            .RunAsync();
    }
}