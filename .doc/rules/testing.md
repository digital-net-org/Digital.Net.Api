# Testing rules

## Isolation per test method
Each integration test has **its own database state per test method** (not per class).
A single PostgreSQL container is shared across the whole test session
(`PostgresFixture` with `[ClassDataSource(Shared = SharedType.PerTestSession)]`)
and [Respawn](https://github.com/jbogard/Respawn) truncates every table in the
`digital_net`, `digital_net_cms`, `crud_test`, and `public` schemas before each
test runs (via `DatabaseUnitTest.InitializeAsync` / `TestApplication.InitializeAsync`).

## Unit tests are mandatory
Since the project is a library, development relies exclusively on unit tests.
