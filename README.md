<h1>
    <img width="300" src="https://raw.githubusercontent.com/digital-net-org/.github/refs/heads/master/assets/logo_v2025.svg">
</h1>
<div justify="center">
    <a href="https://www.docker.com/"><img src="https://img.shields.io/badge/Docker-blue.svg?color=1d63ed"></a>
        <a href="https://dotnet.microsoft.com/en-us/languages/csharp"><img src="https://img.shields.io/badge/C%23-blue.svg?color=622075"></a>
    <a href="https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-9/overview?WT.mc_id=dotnet-35129-website"><img src="https://img.shields.io/badge/Dotnet-blue.svg?color=4f2bce"></a>
</div>

_@digital-net-org/digital-api_

Digital Net Rest API solution.

## :memo: Configuration

You can configurate the application using environment variables and volume while mounting the docker image.

### :whale2: Dockerfile

#### Environment variables

| Variable                 | Type       | Description                                                            |
|--------------------------|------------|------------------------------------------------------------------------|
| `ASPNETCORE_ENVIRONMENT` | `string`   | Supports `Development` or `Production`.                                |
| `DB_CONNECTION_STRING`   | `string`   | The Database connection string.                                        |
| `ALLOWED_ORIGINS`        | `string[]` | Cors allowed origins.                                                  |
| `APP_URL`                | `string`   | Url the API is served from.                                            |
| `AUDIENCE`               | `string[]` | The Database connection string.                                        |
| `JWT_EXP_BEARER`         | `number`   | The validity duration of the bearer token, expressed in milliseconds.  |
| `JWT_EXP_REFRESH`        | `number`   | The validity duration of the refresh token, expressed in milliseconds. |
| `JWT_COOKIE_NAME`        | `string`   | The name of the cookie containing the token.                           |
| `PASSWORD_REGEX`         | `string`   | Admins password regular expression.                                    |
