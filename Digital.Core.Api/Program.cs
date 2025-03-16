using Digital.Core.Api.Seeds;
using Digital.Lib.Net.Authentication;
using Digital.Lib.Net.Files;
using Digital.Lib.Net.Mvc;
using Digital.Lib.Net.Sdk;

namespace Digital.Core.Api;

public sealed class Program
{
    public const string AppName = "Digital.Core.Api";
    private static async Task Main(string[] args)
    {
        var app = WebApplication.CreateBuilder(args)
            .AddDigitalSdk(AppName)
            .AddDigitalAuthentication()
            .AddDigitalFilesServices()
            .AddDigitalMvc()
            .AddCoreServices()
            .AddDataSeeds()
            .Build();

        await app
            .UseDigitalSdk(AppName)
            .RunAsync();
    }
}