# Conventions

## Language

Everything in the project must be written in **English**: code, comments, documentation, commit messages, branch names, etc.

## Formatting reference

Formatting and C# style rules are defined in the `.editorconfig` file at the project root.
Do not duplicate those rules here. Always refer to and respect them.

## Naming conventions

| Element | Convention | Example |
|---------|-----------|---------|
| Assembly | `Digital.Net.Api.{Domain}` | `Digital.Net.Api.Entities` |
| Test assembly | `Digital.Net.Api.{Domain}.Test` | `Digital.Net.Api.Core.Test` |
| Namespace | Follows directory structure | `Digital.Net.Api.Entities.Models.Users` |
| Endpoints | `{Domain}Endpoints` (static class) | `UserEndpoints`, `PageEndpoints` |
| DI injector | `{Domain}Injector` or `{Domain}ServicesInjector` (static class) | `DigitalEntitiesInjector`, `ServicesInjector` |
| Service | `I{Name}Service` / `{Name}Service` | `IUserService` / `UserService` |
| Repository | `IRepository<T>` / `Repository<T>` | Generic, no entity-specific repos |
| DTO | `{Entity}Dto` | `UserDto`, `PageDto` |
| Payload (input) | `{Entity}Payload` or `{Action}Payload` | `PagePayload`, `LoginPayload` |
| Query (pagination filters) | `{Entity}Query` | `PageQuery`, `UserQuery` |
| Entity | Singular PascalCase | `User`, `Page`, `Document` |
| DB table | Singular, `[Table("Name")]` attribute | `[Table("User")]` |

## Code style

### File-scoped namespaces
Always use file-scoped namespaces:
```csharp
namespace Digital.Net.Api.Entities.Models.Users;
```

### Primary constructors for injection
Use primary constructors for dependency injection:
```csharp
public class CrudService<T>(
    IRepository<T> repository,
    ICrudValidationService crudValidationService
) : ICrudService<T>
    where T : Entity
```

### Expression-bodied members
Prefer expression-bodied members when the logic fits on a single line:
```csharp
public void Delete(T entity) => context.Set<T>().Remove(entity);
```

### DTOs
DTOs, Payloads, and Queries must **always** be declared in `Digital.Net.Api.Controllers/Dto/`.
If a service needs a similar input object, it must define its own parameter type within the service assembly — never reference a DTO from Controllers.

A DTO must always have:
1. An **empty constructor** (for deserialization)
2. A **constructor taking the source entity** as parameter

```csharp
public class UserDto
{
    public UserDto() { }

    public UserDto(User userModel)
    {
        Id = userModel.Id;
        Username = userModel.Username;
    }

    public Guid Id { get; init; }
    public string? Username { get; init; }
}
```

### Minimal API endpoints
Endpoints are **static classes** with:
- A public `Map{Domain}Endpoints` method returning `IEndpointRouteBuilder`
- **Private static methods** as handlers
- Route groups via `MapGroup`
- OpenAPI documentation via `.WithDoc()`

```csharp
public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/user").WithTags("User");
        group.MapGet("/self", GetSelf);
        return app;
    }

    private static IResult GetSelf(IUserContextService userContextService) { ... }
}
```

### Endpoint return values
Always return a `Result` or `Result<T>` wrapped in `Results.Ok()`, `Results.BadRequest()`, etc.
Never let uncaught exceptions bubble up from an endpoint.

### Mapping
Use the Core `Mapper`:
- `Mapper.TryMap<T, TM>()` — tries constructor then reflection
- `Mapper.MapFromConstructor<T, TM>()` — constructor-based mapping
- `Mapper.Map<T, TM>()` — reflection-based mapping
- **Do not** use AutoMapper or any other external mapping library.

## File organization

Each assembly organizes its files by **functional domain** (subdirectories):
```
Digital.Net.Api.Services/
├── Documents/
│   ├── DocumentService.cs
│   ├── IDocumentService.cs
│   └── DocumentServicesInjector.cs
├── Users/
│   ├── UserService.cs
│   ├── IUserService.cs
│   └── UserServicesInjector.cs
└── ServicesInjector.cs   ← Parent injector composing sub-injectors
```
