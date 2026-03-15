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
Install the nuget package in your project and call the `AddDigitalNet()` and `UseDigitalNet()` methods.

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
| ___Domain___                             <br/>Describes your application domain. Used to **prefix Cookies**, setup JWT **Audience/Issuer** and all subdomains will be added the allowed **CORS policies** | `string`   | **Mandatory**            | 
| ___CorsAllowedOrigins___                 <br/>All entries will be added the allowed **CORS policies** _(be aware that Domain is automatically added to allowed origins)_                                  | `string[]` | `[]`                     |
| ___Database:ConnectionString___          <br/>Postgres Database connection string formated like `"Host=host;Port=5432;Database=db;Username=usr;Password=psw"`                                             | `string`   | **Mandatory**            |
| ___Database:UseSqlite___                 <br/>Use an Sqlite Database if true. Used for Integration tests                                                                                                  | `boolean`  | `false`                  |
| ___FileSystemPath___                     <br/>Path to folder where the application will save uploaded files                                                                                               | `string`   | `"/digital_net_storage"` |
| ___Auth:JwtRefreshExpiration___          <br/>Refresh token expiration expressed in milliseconds                                                                                                          | `number`   | `1800000`                |
| ___Auth:JwtBearerExpiration___           <br/>Bearer token expiration expressed in milliseconds                                                                                                           | `number`   | `300000`                 |
| ___Auth:JwtSecret___                     <br/>Secret for Jwt configuration, must be a least 46 characters long                                                                                            | `string`   | _Random string_          |
| ___Git:Origin___                         <br/>Optional string that will be returned in `/`                                                                                                                | `string`   | _Random string_          |
| ___Git:CommitSha___                      <br/>Optional string that will be returned in `/`                                                                                                                | `string`   | _Random string_          |
| ___Git:Release___                        <br/>Optional string that will be returned in `/`                                                                                                                | `string`   | _Random string_          |
