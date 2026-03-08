# Testing

## Framework

The project uses **[TUnit](https://tunit.dev/docs)** (not xUnit, not NUnit).

## Commands

> **Important**: The standard `dotnet test` command will not work with our current stack. You must execute the test projects directly using `dotnet run`.

```bash
# Run tests for a specific project
dotnet run --project Digital.Net.Api.Test/Digital.Net.Api.Test.csproj
```

> **Strict rule**: tests must always run in **parallel**. Never use sequential execution options.

## Rules

### Isolation per test method
Each integration test has **its own database / application context per test method** (not per class).
This is guaranteed by `[ClassDataSource<TestApplication>]` which creates an `ApplicationFactory` with a unique SQLite in-memory database per instance.

### Unit tests are mandatory
Since the project is a library, development relies exclusively on unit tests.
