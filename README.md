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

## Authentication

Three schemes coexist, resolved in this order by the authorization filter:

| Scheme | Carrier | For |
|---|---|---|
| `ApiKey` | `DN-Api-Key` header | machine-to-machine, per user |
| `Session` | `dn_session` cookie | browser clients (the back-office) |
| `Application` | `DN-Application-Key` header | one trusted server-side consumer |

**Sessions are opaque and server-side.** `POST authentication/user/login` stores a CSPRNG id hashed
with SHA-256 in the `Session` table and returns it only through a `Set-Cookie` — `HttpOnly`, `Secure`,
`Path=/`, no `Domain` (host-only on the API host). The body carries no identity: `GET user/self` is the
single source of truth. Nothing reaches JavaScript, so an XSS cannot steal a session.

A session has two deadlines: an **idle** one that slides as it is used (at most one write per 10 min)
and an **absolute** one that is never extended. Revocation is immediate — logout deletes the row, and
the next request is rejected. A password change drops every session of the account and issues a fresh
one to the caller.

**CSRF.** A cookie is attached by the browser on its own, so every *mutating* request authenticated by
session must also carry the `DN-Requested-With` header. Only its presence is checked: a cross-site
context cannot set a custom header without a preflight, which the CORS policy grants to
`CorsAllowedOrigins` alone. Safe methods (GET/HEAD/OPTIONS) are exempt, so `<img src>` and the SSE
stream keep working, and machine-to-machine schemes are exempt too — their holder is not a browser.
Rejection answers **403**, never 401, so a transport bug is not mistaken for an expired session.

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
variables replace `:` with `__` (e.g. `Auth:ApplicationKey` → `Auth__ApplicationKey`,
`Database:ConnectionString` → `Database__ConnectionString`).

`ApplicationDomain`, `CorsAllowedOrigins`, `Database:ConnectionString` and
`Auth:ApplicationKey` are **validated at startup**: the host throws if any of them is
missing or blank. It also throws when
`Auth:SessionIdleExpiration` exceeds `Auth:SessionAbsoluteExpiration`, which would
silently disable the idle window.

| Accessor                                                                                                                                                                                                                                                                                                            | Type       | Default value            |
|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------------|--------------------------|
| ___ASPNETCORE_ENVIRONMENT___<br/>Runtime profile. Selects the `appsettings.{Environment}.json` file and toggles env-specific behaviour: the Scalar UI and OpenAPI document are exposed only in `Development`, and the rate limiter is disabled in `Test`. One of `Development` / `Staging` / `Production` / `Test`. | `string`   | `Development`            |
| ___ApplicationName___<br/>Name of your application, returned by the `GET /` endpoint.                                                                                                                                                                                                                               | `string`   | `""`                     |
| ___ApplicationDomain___<br/>Registrable domain the application lives under, e.g. `safaridigital.fr` (`localhost` in dev). Never used to allow anything by itself — it only tells whether an allowed origin is same-site with the API, which decides the session cookie's `SameSite`.                                | `string`   | **Mandatory**            |
| ___CorsAllowedOrigins___<br/>The **only** origins allowed by the **CORS** policy — nothing is inferred. An empty list is refused: with credentialed requests, no browser client could reach the API.                                                                                                                | `string[]` | **Mandatory**            |
| ___Database:ConnectionString___<br/>Postgres connection string, e.g. `"Host=host;Port=5432;Database=db;Username=usr;Password=psw"`. Shared by every context (each uses its own schema).                                                                                                                             | `string`   | **Mandatory**            |
| ___FileSystemPath___<br/>Directory where uploaded files (documents, media) are stored.                                                                                                                                                                                                                              | `string`   | `"/digital_net_storage"` |
| ___Auth:ApplicationKey___<br/>Shared secret for system-to-system **Application** authentication (e.g. a Next.js frontend), sent via the `DN-Application-Key` header.                                                                                                                                                | `string`   | **Mandatory**            |
| ___Auth:SessionIdleExpiration___<br/>How long a session survives without being used, in milliseconds. Slides forward as the session is used, at most once per 10 min.                                                                                                                                               | `number`   | `7200000` _(2 h)_        |
| ___Auth:SessionAbsoluteExpiration___<br/>Hard session lifetime, in milliseconds. Never extended, however active the session is. Also the session cookie's `Expires`.                                                                                                                                                | `number`   | `604800000` _(7 d)_      |
| ___Audit:RetentionDays___<br/>Retention window, in days, for audit data. The background `RetentionPurgeService` deletes `EntityMutation` (all schemas) and `AuthEvent` rows older than this (expired `Session`s are purged regardless).                                                                             | `number`   | `90`                     |
| ___ForwardedHeaders:KnownProxies___<br/>IP addresses of the reverse proxies trusted to set `X-Forwarded-For` / `X-Forwarded-Proto`. When neither this nor `ForwardedHeaders:KnownIPNetworks` is set, the headers are **ignored** and the TCP connection address is used.                                            | `string[]` | `[]`                     |
| ___ForwardedHeaders:KnownIPNetworks___<br/>Same trust list as `ForwardedHeaders:KnownProxies`, expressed as CIDR ranges (e.g. `"172.18.0.0/16"` for a Docker network where the reverse proxy gets a dynamic address).                                                                                               | `string[]` | `[]`                     |
| ___ForwardedHeaders:ForwardLimit___<br/>Number of proxy hops in front of the API (1 = a single reverse proxy). Only that many entries of `X-Forwarded-For` are consumed, so the client-supplied part of the header is never trusted.                                                                                | `number`   | `1`                      |
| ___Git:Origin___<br/>Optional build metadata, returned by `GET /`.                                                                                                                                                                                                                                                  | `string`   | `""`                     |
| ___Git:CommitSha___<br/>Optional build metadata, returned by `GET /`.                                                                                                                                                                                                                                               | `string`   | `""`                     |
| ___Git:Release___<br/>Optional build metadata, returned by `GET /`.                                                                                                                                                                                                                                                 | `string`   | `""`                     |
