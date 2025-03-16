using Digital.Core.Api.Seeds;
using Digital.Core.Api.Services.Users;
using Digital.Lib.Net.Authentication;
using Digital.Lib.Net.Core.Environment;
using Digital.Lib.Net.Entities.Context;
using Digital.Lib.Net.Entities.Seeds;
using Digital.Lib.Net.Files;
using Digital.Lib.Net.Mvc;
using Digital.Lib.Net.Sdk;
using Digital.Lib.Net.Sdk.Bootstrap;

namespace Digital.Core.Api;

public sealed class Program
{
    public const string AppName = "Digital.Core.Api";
    private static async Task Main(string[] args)
    {
        var app = WebApplication.CreateBuilder(args)
            .AddDigitalSdk(AppName)
            .AddDataSeeds()
            .AddDigitalAuthentication()
            .AddDigitalFilesServices()
            .AddDigitalMvc()
            .AddCoreServices()
            .Build();

        await app
            .UseDigitalSdk(AppName)
            .RunAsync();
    }
}