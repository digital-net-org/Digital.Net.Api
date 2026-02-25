# Workflows

Guides for common operations on the project.

## Adding an entity

1. **Create the model** in `Digital.Net.Api.Entities/Models/{Domain}/`
   - Inherit from `Entity` (provides `Id`, `CreatedAt`, `UpdatedAt`)
   - Define entity properties using **EF Core attributes** (`[Required]`, `[MaxLength]`, `[Column]`, `[Table]`, etc.) and **custom attributes** (`[ReadOnly]`, `[Secret]`, `[RegexValidation]`, `[DataFlag]`)
   - The `CrudService` uses these attributes to automatically generate an entity schema exposed via the API
   - If a new entity rule is needed, it must be implemented as a **custom attribute** in `Digital.Net.Api.Entities/Attributes/`

2. **Register the DbSet** in `DigitalContext`
   ```csharp
   public DbSet<NewEntity> NewEntities { get; init; }
   ```

3. **Configure relationships** in `DigitalContext.OnModelCreating()` if needed

4. **Generate the migration** using the provided script:
   ```bash
   pwsh .scripts/EfMigrate.ps1 MigrationName
   ```
   The script reads the connection string from `Digital.Net.Tests.Program/appsettings.Development.json` (`Database:ConnectionString`).

   To **revert the last migration**:
   ```bash
   pwsh .scripts/EfUndoMigrate.ps1
   ```

5. **Optional: create a seed** — implement `ISeed` and register it in DI

## Adding a standard CRUD endpoint

For a full CRUD using the generic framework:

1. **Create the DTO** in `Digital.Net.Api.Controllers/Dto/`
   - Empty constructor + `(Entity entity)` constructor
   - Properties as `{ get; init; }` or `{ get; set; }`

2. **Create the Payload** (for POST) in `Digital.Net.Api.Controllers/Dto/`

3. **Create the Query** (for filtered pagination) in `Digital.Net.Api.Controllers/Dto/`

4. **Create the endpoint** in `Digital.Net.Api.Controllers/Controllers/{Domain}Endpoints.cs`
   ```csharp
   public static class NewEntityEndpoints
   {
       public static IEndpointRouteBuilder MapNewEntityEndpoints(this IEndpointRouteBuilder app)
       {
           var group = app.MapGroup("/new-entity").WithTags("NewEntity");

           group.MapCrudGet<NewEntity, NewEntityDto>("")
               .RequireAuthentication(AuthorizeType.Any);

           group.MapPaginationGet<NewEntity, NewEntityDto, NewEntityQuery>("", PaginationFilter)
               .RequireAuthentication(AuthorizeType.Any);

           group.MapCrudPatch<NewEntity>("")
               .RequireAuthentication(AuthorizeType.Any);

           group.MapCrudPost<NewEntity, NewEntityPayload>("")
               .RequireAuthentication(AuthorizeType.Any);

           group.MapCrudDelete<NewEntity>("")
               .RequireAuthentication(AuthorizeType.Any);

           return app;
       }
   }
   ```

5. **Register the endpoint** in `ControllersMapper.UseDigitalEndpoints()`
   ```csharp
   app.MapNewEntityEndpoints();
   ```

## Adding a custom endpoint

1. Add the handler as a **private static method** in the appropriate `*Endpoints.cs` file
2. Map the handler in the `Map*Endpoints` method of the same file
3. Add OpenAPI documentation via `.WithDoc()`
4. Handle errors via `Result` / `Result<T>` and return the appropriate HTTP status code

## Adding a service

1. **Create the interface** `I{Name}Service.cs` in `Digital.Net.Api.Services/{Domain}/`
2. **Create the implementation** `{Name}Service.cs` in the same directory
3. **Create the injector** `{Domain}ServicesInjector.cs` or add registration to an existing injector
   ```csharp
   services.AddScoped<INameService, NameService>();
   ```
4. **Compose in the parent** — if new injector, call it from `ServicesInjector.AddDigitalServices()`
5. **Write unit tests**

## Adding a DTO

DTOs, Payloads, and Queries must always live in `Digital.Net.Api.Controllers/Dto/`. If a service needs a similar input object, create a dedicated parameter type within the service assembly instead.

1. Create the file in `Digital.Net.Api.Controllers/Dto/`
2. Add an empty constructor
3. Add a constructor taking the source entity
4. Declare properties as `{ get; init; }` for exposed fields
5. Exclude sensitive fields (Password, etc.)
