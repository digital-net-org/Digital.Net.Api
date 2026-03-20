<h1 align="center">
    <img width="256" src="logo.png">
</h1>
<p align="center">
    Digital Net Rest API solution.
</p>
<p align="center">
    <a href="https://www.docker.com/"><img src="https://img.shields.io/badge/Docker-blue.svg?color=1d63ed"></a>
        <a href="https://dotnet.microsoft.com/en-us/languages/csharp"><img src="https://img.shields.io/badge/C%23-blue.svg?color=622075"></a>
    <a href="https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-10/overview"><img src="https://img.shields.io/badge/Dotnet_10-blue.svg?color=4f2bce"></a>
</p>

## 📦 Installation

### Core

Install the `Digital.Net.Core` NuGet package and call `AddDigitalNet()` / `UseDigitalNet()` in your entry point.

```csharp
public sealed class Program
{
    private static async Task Main(string[] args)
    {
        var app = WebApplication.CreateBuilder(args)
            .AddDigitalNet()
            .Build();

        await app
            .UseDigitalNet()
            .RunAsync();
    }
}
```

This registers authentication (JWT + API Key + Application), user management, CRUD services, audit logging, document storage, rate limiting, and OpenAPI documentation.

### CMS module (optional)

Add the `Digital.Net.Cms` project (or package) and chain `AddDigitalCms()` / `UseDigitalCms()`.

```csharp
public sealed class Program
{
    private static async Task Main(string[] args)
    {
        var app = WebApplication.CreateBuilder(args)
            .AddDigitalNet()
            .AddDigitalCms()
            .Build();

        await app
            .UseDigitalNet()
            .UseDigitalCms()
            .RunAsync();
    }
}
```

This adds headless CMS capabilities: pages, articles, tags, media (with on-demand image resizing), CSS/JS sheets, and sitemap data endpoints.

## 📝 Configuration

You can configurate the application using environment variables.

The application SDK will automatically load environment variables from the `appsettings.*.json` file located in the root 
of the project and override the values set in environment variables.

The loadings order is:
1. `appsettings.json`
2. `appsettings.{Environment}.json`
3. `appsettings.local.json`
4. Environment variables

#### Environment variables
| Accessor                                                                                                                                                                                                  | Type       | Default value            |
|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------------|--------------------------|
| ___ApplicationName___                    <br/>Name of your application, returned by the `GET /` endpoint                                                                                                 | `string`   | `""`                     |
| ___Domain___                             <br/>Describes your application domain. Used to **prefix Cookies**, setup JWT **Audience/Issuer** and all subdomains will be added the allowed **CORS policies** | `string`   | **Mandatory**            |
| ___CorsAllowedOrigins___                 <br/>All entries will be added the allowed **CORS policies** _(be aware that Domain is automatically added to allowed origins)_                                  | `string[]` | `[]`                     |
| ___Database:ConnectionString___          <br/>Postgres Database connection string formated like `"Host=host;Port=5432;Database=db;Username=usr;Password=psw"`                                             | `string`   | **Mandatory**            |
| ___Database:UseSqlite___                 <br/>Use an Sqlite Database if true. Used for Integration tests                                                                                                  | `boolean`  | `false`                  |
| ___FileSystemPath___                     <br/>Path to folder where the application will save uploaded files                                                                                               | `string`   | `"/digital_net_storage"` |
| ___Auth:JwtRefreshExpiration___          <br/>Refresh token expiration expressed in milliseconds                                                                                                          | `number`   | `3600000`                |
| ___Auth:JwtBearerExpiration___           <br/>Bearer token expiration expressed in milliseconds                                                                                                           | `number`   | `300000`                 |
| ___Auth:JwtSecret___                     <br/>Secret for Jwt configuration, must be a least 46 characters long                                                                                            | `string`   | _Random string_          |
| ___Auth:ApplicationKey___                <br/>Shared secret for system-to-system **Application** authentication (e.g. Next.js frontend). Sent via the `DN-Application-Key` header                       | `string`   | _None (disabled)_        |
| ___Git:Origin___                         <br/>Optional string that will be returned in `GET /`                                                                                                            | `string`   | `""`                     |
| ___Git:CommitSha___                      <br/>Optional string that will be returned in `GET /`                                                                                                            | `string`   | `""`                     |
| ___Git:Release___                        <br/>Optional string that will be returned in `GET /`                                                                                                            | `string`   | `""`                     |
