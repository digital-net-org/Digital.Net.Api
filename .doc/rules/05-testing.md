# Testing

## Framework

The project uses **[TUnit](https://tunit.dev/docs)** (not xUnit, not NUnit).

## Commands

```bash
# Run all tests (always in parallel)
dotnet test

# Run tests for a specific project
dotnet test Digital.Net.Api.Core.Test
dotnet test Digital.Net.Api.Controllers.Tests
```

> **Strict rule**: tests must always run in **parallel**. Never use sequential execution options.

## Rules

### Isolation per test method
Each integration test has **its own database / application context per test method** (not per class).
This is guaranteed by `[ClassDataSource<TestApplication>]` which creates an `ApplicationFactory` with a unique SQLite in-memory database per instance.

### Unit tests are mandatory
Since the project is a library, development relies exclusively on unit tests.

## Test infrastructure

### Hierarchy

```
Digital.Net.Tests.Core/
├── UnitTest.cs                    ← Base class for unit tests (sets env to Test)
├── SqliteInMemoryHelper.cs        ← SQLite helper
├── Factories/
│   ├── ApplicationFactory.cs      ← WebApplicationFactory<DigitalProgram> with SQLite
│   ├── TestApplication.cs         ← IAsyncInitializer/IAsyncDisposable wrapper
│   ├── TestSeed.cs                ← Test seed
│   └── Data/
│       ├── TestUserDataFactory.cs ← Test User creation factory
│       ├── TestPageDataFactory.cs ← Test Page creation factory
│       └── Records/
│           └── TestUserPayload.cs ← User payload record
├── Sdk/
│   ├── AuthenticationApi.cs       ← HTTP helpers for auth endpoints
│   ├── UserApi.cs                 ← HTTP helpers for user endpoints
│   ├── PageApi.cs                 ← HTTP helpers for page endpoints
│   ├── AdministrationApi.cs       ← HTTP helpers for admin endpoints
│   ├── RootApi.cs                 ← HTTP helpers for root endpoint
│   └── ValidationApi.cs           ← HTTP helpers for validation endpoints

Digital.Net.Tests.Program/
└── DigitalProgram.cs              ← Host program for WebApplicationFactory
```

### Key components

| Component | Role |
|-----------|------|
| `UnitTest` | Abstract base class for unit tests, sets environment to `Test` |
| `ApplicationFactory` | Factory derived from `WebApplicationFactory<DigitalProgram>`, unique SQLite config, logging disabled |
| `TestApplication` | Disposable wrapper with helpers: `CreateClient()`, `CreateUser()`, `AsLogged()`, `GetService<T>()`, `GetRepository<T>()` |
| SDK helpers | Extension methods on `HttpClient` to call endpoints (e.g. `client.Login()`, `client.GetSelf()`, `client.PatchSelf()`) |

## Integration test pattern

```csharp
public class UserEndpointsTest
{
    [ClassDataSource<TestApplication>]
    public required TestApplication Application { get; init; }

    [Test]
    public async Task GetSelf_ShouldReturnAuthenticatedUser()
    {
        var user = Application.CreateUser(new TestUserPayload { IsActive = true });
        var client = Application.CreateClient();
        await client.Login(user);

        var response = await client.GetSelf();
        var result = await response.Content.ReadFromJsonAsync<Result<UserDto>>();

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
        await Assert.That(result!.Value!.Id).IsEqualTo(user.Id);
    }
}
```

### Key points
- `[ClassDataSource<TestApplication>]` to get an isolated instance
- Create test data via `Application.CreateUser()` and data factories
- Create an `HttpClient` via `Application.CreateClient()`
- Authenticate via `client.Login(user)` or `Application.AsLogged(client, user)`
- Call endpoints via SDK helpers
- TUnit assertions: always `await Assert.That(...)`

## Test naming convention

```
{TestedMethod}_{Scenario}
```

Examples:
- `GetSelf_ShouldReturnAuthenticatedUser`
- `PatchSelf_ShouldRejectReadOnlyField_IsAdmin`
- `UpdatePassword_ShouldUpdatePassword`
