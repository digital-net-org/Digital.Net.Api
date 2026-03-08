# Conventions

## Language

Everything in the project must be written in **English**: code, comments, documentation, commit messages, branch names, etc.

## Formatting reference

Formatting and C# style rules are defined in the `.editorconfig` file at the project root.
Do not duplicate those rules here. Always refer to and respect them.

## Naming conventions

| Element | Convention | Example |
|---------|-----------|---------|
| Assembly | `Digital.Net.Api.{Domain}` | 
| Test assembly | `Digital.Net.Api.{Domain}.Test`|
| Endpoints class | `{Domain}Endpoints` (static class)|
| Endpoints mapping | `Map{Domain}Endpoints` method |
| DI injector | `{Domain}Injector` or `{Domain}ServicesInjector` (static class) |
| Service | `I{Name}Service` / `{Name}Service` |
| DTO | `{Entity}Dto` | 
| Payload (input) | `{Entity}Payload` or `{Action}Payload` |
| Query (pagination filters) | `{Entity}Query` | 
| Entity | Singular PascalCase | 
| DB table | Singular, `[Table("Name")]` attribute |

## DTOs
DTOs, Payloads, and Queries must **always** be declared in an _"Endpoints"_ namespace.
If a service needs a similar input object, it must define its own parameter type within the service assembly — never reference a DTO from Endpoints.

A DTO must always have:
1. An **empty constructor** (for deserialization)
2. A **constructor taking the source entity** as parameter (for mapping)

## Endpoint return values
Always return a `Result` or `Result<T>` wrapped in a `TypedResults`.
Never let uncaught exceptions bubble up from an endpoint.
