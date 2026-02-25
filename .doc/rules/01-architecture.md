# Architecture

Digital.Net.Api is a **NuGet library** intended to be consumed by a .NET API application.
It provides a complete SDK (authentication, generic CRUD, document management, auditing, etc.) that a host application activates via `AddDigitalSdk()` / `UseDigitalSdk()`.

- **Runtime**: .NET 10
- **Language**: C# 14
- **ORM**: Entity Framework Core
- **Database**: PostgreSQL (production), SQLite (tests)
- **API style**: Minimal API
- **Tests**: TUnit
- **API documentation**: OpenAPI + [Scalar](https://github.com/ScalarHQ/scalar)

## Assemblies

```
Digital.Net.Api.Sdk              ← Entry point, composes all modules
├── Digital.Net.Api.Controllers  ← Minimal API endpoints + DTOs
├── Digital.Net.Api.Services     ← Business logic (Users, Documents, HttpContext)
├── Digital.Net.Api.Authentication ← JWT, API Keys, authorization filters
├── Digital.Net.Api.Auditing     ← Auditing services
├── Digital.Net.Api.Entities     ← EF Core models, Repository<T>, CrudService<T>, migrations, seeds
└── Digital.Net.Api.Core         ← Shared utilities (Result, Mapper, Formatters, Settings, Exceptions)
```

### Test projects

```
Digital.Net.Tests.Core            ← Shared test infrastructure (ApplicationFactory, TestApplication, SDK helpers, data factories)
Digital.Net.Tests.Program         ← Test host program (DigitalProgram)
Digital.Net.Api.Core.Test         ← Unit tests for Core
Digital.Net.Api.Entities.Test     ← Unit tests for Entities
Digital.Net.Api.Services.Test     ← Unit tests for Services
Digital.Net.Api.Controllers.Tests ← Integration tests for endpoints
Digital.Net.Api.Authentication.Tests ← Integration tests for auth
```

## Dependency injection pattern

Each assembly exposes a **static Injector** that registers its services into the DI container:

| Assembly | Injector | Method |
|----------|----------|--------|
| Entities | `DigitalEntitiesInjector` | `AddDigitalEntities()` |
| Services | `ServicesInjector` | `AddDigitalServices()` |
| Authentication | `AuthenticationInjector` | `AddDigitalAuthenticationServices()` |
| Auditing | `AuditingInjector` | `AddDigitalAuditServices()` |
| Sdk | `DigitalSdkInjector` | `AddDigitalSdk()` / `UseDigitalSdk()` |

Sub-domain injectors (e.g. `UserServicesInjector`, `HttpContextServicesInjector`) are composed by their parent injector.

## Endpoint composition

Endpoints are registered via `ControllersMapper.UseDigitalEndpoints()` which calls each `Map*Endpoints()`:

```
ControllersMapper.UseDigitalEndpoints()
├── MapRootEndpoints()
├── MapAuthenticationEndpoints()
├── MapUserEndpoints()
├── MapAdministrationEndpoints()
├── MapPageEndpoints()
└── MapValidationEndpoints()
```

## Key patterns

| Pattern | Implementation |
|---------|---------------|
| Generic repository | `Repository<T>` / `IRepository<T>` — EF Core data access |
| Generic CRUD | `CrudService<T>` / `ICrudService<T>` — CRUD operations with validation |
| CRUD endpoints | `CrudEndpointExtensions` — `MapCrudGet`, `MapCrudPost`, `MapCrudPatch`, `MapCrudDelete`, `MapPaginationGet` |
| Messaging | `Result` / `Result<T>` — response envelope with `Errors` and `Infos` |
| Mapping | `Mapper` — mapping via constructor or reflection (not AutoMapper) |
| Entity validation | Custom attributes (`ReadOnly`, `Secret`, `RegexValidation`) + `CrudValidationService` |
| Seeds | `ISeed` interface + `Seeder` for initial database seeding |
