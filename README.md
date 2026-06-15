<h1 align="center">
    <img width="256" src="logo.png">
</h1>
<p align="center">
    Digital Net REST API framework library.
</p>
<p align="center">
    <a href="https://www.docker.com/"><img src="https://img.shields.io/badge/Docker-blue.svg?color=1d63ed"></a>
    <a href="https://dotnet.microsoft.com/en-us/languages/csharp"><img src="https://img.shields.io/badge/C%23-blue.svg?color=622075"></a>
    <a href="https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-10/overview"><img src="https://img.shields.io/badge/Dotnet_10-blue.svg?color=4f2bce"></a>
</p>

---

## Overview

Digital.Net is a .NET 10 / ASP.NET Core framework that bootstraps a REST API with batteries included: 
authentication, user management, generic CRUD services, audit logging, document storage, rate limiting, and much more.

## Getting Started (contributors)
### Prerequisites

- **.NET SDK** 10
- **PostgreSQL** 15+
- **Docker** or **Podman** (the test suite spins up an ephemeral
  PostgreSQL container via [Testcontainers](https://dotnet.testcontainers.org/))

### Clone
This library is only provided as a Git submodule.
```bash
git clone --recurse-submodules git@github.com:digital-net-org/Digital.Net.Api.git
```

### Run the tests

```bash
dotnet test Digital.Net.slnx
```

The test suite uses [Testcontainers](https://dotnet.testcontainers.org/) to
start a single ephemeral PostgreSQL container shared by every test, with
[Respawn](https://github.com/jbogard/Respawn) truncating the tables between
tests. Docker just works out of the box.

#### Running on Podman

If you use Podman (rootless), enable the Docker-compatible socket and point
Testcontainers at it:

```bash
systemctl --user enable --now podman.socket
export DOCKER_HOST="unix://${XDG_RUNTIME_DIR}/podman/podman.sock"
export TESTCONTAINERS_RYUK_DISABLED=true
```

Ryuk (the cleanup container) is disabled because it does not play well with
rootless Podman. The Postgres container still cleans itself up via
`WithCleanUp(true)` when the test session exits.

## Configuration

Configure the application via environment variables or `appsettings.*.json`.
Files at the project root are loaded automatically; environment variables
override file values.

Loading order:
1. `appsettings.json`
2. `appsettings.{Environment}.json`
3. `appsettings.local.json`
4. Environment variables

### Environment variables

The table below lists **every** configuration accessor read by the framework.
Hierarchical keys use `:` in `appsettings.*.json`; as real OS environment
variables replace `:` with `__` (e.g. `Auth:JwtSecret` → `Auth__JwtSecret`,
`Database:ConnectionString` → `Database__ConnectionString`).

`Domain`, `Database:ConnectionString`, `Auth:JwtSecret` and `Auth:ApplicationKey`
are **validated at startup**: the host throws if any of them is missing or blank.

| Accessor                                                                                                                                                                                                                                                                                                            | Type       | Default value            |
|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------------|--------------------------|
| ___ASPNETCORE_ENVIRONMENT___<br/>Runtime profile. Selects the `appsettings.{Environment}.json` file and toggles env-specific behaviour: the Scalar UI and OpenAPI document are exposed only in `Development`, and the rate limiter is disabled in `Test`. One of `Development` / `Staging` / `Production` / `Test`. | `string`   | `Development`            |
| ___ApplicationName___<br/>Name of your application, returned by the `GET /` endpoint.                                                                                                                                                                                                                               | `string`   | `""`                     |
| ___Domain___<br/>Application domain. Used to **prefix the refresh cookie**, set the JWT **Audience/Issuer**, and add every subdomain to the allowed **CORS** origins.                                                                                                                                               | `string`   | **Mandatory**            |
| ___CorsAllowedOrigins___<br/>Extra origins added to the allowed **CORS** policy _(`Domain` and its subdomains are already added automatically)_.                                                                                                                                                                    | `string[]` | `[]`                     |
| ___Database:ConnectionString___<br/>Postgres connection string, e.g. `"Host=host;Port=5432;Database=db;Username=usr;Password=psw"`. Shared by every context (each uses its own schema).                                                                                                                             | `string`   | **Mandatory**            |
| ___FileSystemPath___<br/>Directory where uploaded files (documents, media) are stored.                                                                                                                                                                                                                              | `string`   | `"/digital_net_storage"` |
| ___Auth:JwtSecret___<br/>HMAC-SHA256 signing secret for JWTs. Must be at least 46 characters long.                                                                                                                                                                                                                  | `string`   | **Mandatory**            |
| ___Auth:ApplicationKey___<br/>Shared secret for system-to-system **Application** authentication (e.g. a Next.js frontend), sent via the `DN-Application-Key` header.                                                                                                                                                | `string`   | **Mandatory**            |
| ___Auth:JwtBearerExpiration___<br/>Bearer (access) token lifetime, in milliseconds.                                                                                                                                                                                                                                 | `number`   | `300000` _(5 min)_       |
| ___Auth:JwtRefreshExpiration___<br/>Refresh token lifetime, in milliseconds.                                                                                                                                                                                                                                        | `number`   | `3600000` _(1 h)_        |
| ___Audit:RetentionDays___<br/>Retention window, in days, for audit data. The background `RetentionPurgeService` deletes `EntityMutation` (all schemas) and `AuthEvent` rows older than this (expired `ApiToken`s are purged regardless).                                                                            | `number`   | `90`                     |
| ___Git:Origin___<br/>Optional build metadata, returned by `GET /`.                                                                                                                                                                                                                                                  | `string`   | `""`                     |
| ___Git:CommitSha___<br/>Optional build metadata, returned by `GET /`.                                                                                                                                                                                                                                               | `string`   | `""`                     |
| ___Git:Release___<br/>Optional build metadata, returned by `GET /`.                                                                                                                                                                                                                                                 | `string`   | `""`                     |
