<h1>
    <img width="300" src="https://raw.githubusercontent.com/digital-net-org/.github/refs/heads/master/assets/logo_v2025.svg">
</h1>
<div justify="center">
    <a href="https://www.docker.com/"><img src="https://img.shields.io/badge/Docker-blue.svg?color=1d63ed"></a>
        <a href="https://dotnet.microsoft.com/en-us/languages/csharp"><img src="https://img.shields.io/badge/C%23-blue.svg?color=622075"></a>
    <a href="https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-9/overview?WT.mc_id=dotnet-35129-website"><img src="https://img.shields.io/badge/Dotnet-blue.svg?color=4f2bce"></a>
</div>

_@digital-net-org/digital-core-api_

Digital Net Rest API solutions.

Digital-core-api container handles Digital API configuration, users, file and authentication.

## :memo: Configuration

You can configurate the application using environment variables and volume while mounting the docker image.

"Default" prefix means that the value is used to set the default configuration at the first application start. These values are stored in the database and can be modified later using the provided endpoints.

### :whale2: Dockerfile

#### Environment variables
| Accessor                                                                                                                                                                                                  | Type       | Default value            |
|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------------|--------------------------|
| ___Domain___                             <br/>Describes your application domain. Used to **prefix Cookies**, setup JWT **Audience/Issuer** and all subdomains will be added the allowed **CORS policies** | `string`   | **Mandatory**            | 
| ___CorsAllowedOrigins___                 <br/>All entries will be added the allowed **CORS policies** _(be aware that Domain is automatically added to allowed origins)_                                  | `string[]` | `[]`                     |
| ___Database:ConnectionString___          <br/>Postgres Database connection string formated like `"Host=host;Port=5432;Database=db;Username=usr;Password=psw"`                                             | `string`   | **Mandatory**            |
| ___Database:UseSqlite___                 <br/>Use an Sqlite Database if true. Used for Integration tests                                                                                                  | `boolean`  | `false`                  |
| ___Defaults:FileSystemPath___            <br/>Path to folder where the application will save uploaded files                                                                                               | `string`   | `"/digital_net_storage"` |
| ___Defaults:Auth:JwtRefreshExpiration___ <br/>Refresh token expiration expressed in milliseconds                                                                                                          | `number`   | `1800000`                |
| ___Defaults:Auth:JwtBearerExpiration___  <br/>Bearer token expiration expressed in milliseconds                                                                                                           | `number`   | `300000`                 |
| ___Defaults:Auth:JwtSecret___            <br/>Secret for Jwt configuration, must be a least 46 characters long                                                                                            | `string`   | _Random string_          |
