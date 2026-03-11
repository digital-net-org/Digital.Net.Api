## Working with EfCore
Unless specified otherwise, do not try to run or create migrations. If you still need to do so, use the available scripts to do so.
- `.scripts/EfMigrate.ps1`
- `.scripts/EfUndoMigrate.ps1`

## Running tests
The project uses **[TUnit](https://tunit.dev/docs)** (not xUnit, not NUnit).

> **Important**: The standard `dotnet test` command will not work with our current stack. You must execute the test projects directly using `dotnet run`.

```bash
# Run tests for a specific project
dotnet run --project Digital.Net.Api.Test/Digital.Net.Api.Test.csproj
# Run with filters
dotnet run --project Digital.Net.Api.Test/Digital.Net.Api.Test.csproj --treenode-filter "/*/Digital.Net.Api.Test.Endpoints/ApiKeyEndpointsTest/Create_ShouldReturnPlaintextKey/*"
```

> **Strict rule**: tests must always run in **parallel**. Never use sequential execution options.

> **Strict rule**: Only run the test you are working on during the development phase. You can run a global test once the feature is complete to verify that there is no regression.
