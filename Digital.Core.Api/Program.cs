using Digital.Core.Api.Seeds;
using Digital.Lib.Net.Authentication;
using Digital.Lib.Net.Files;
using Digital.Lib.Net.Mvc;
using Digital.Lib.Net.Sdk;
using Digital.Lib.Net.Sdk.Services.Application;

namespace Digital.Core.Api;

public sealed class Program
{
    private static async Task Main(string[] args)
    {
        var app = WebApplication.CreateBuilder(args)
            .SetApplicationName("Digital.Core.Api")
            .AddDigitalSdk()
            .AddDigitalAuthentication()
            .AddDigitalFilesServices()
            .AddDigitalMvc()
            .AddCoreServices()
            .AddDataSeeds()
            .Build();

        await app
            .UseDigitalSdk()
            .RunAsync();
    }
}