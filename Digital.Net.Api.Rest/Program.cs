using Digital.Net.Api.Sdk;
using Digital.Net.Api.Services.Application;

namespace Digital.Net.Api.Rest;

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