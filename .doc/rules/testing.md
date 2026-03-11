# Testing rules

## Isolation per test method
Each integration test has **its own database / application context per test method** (not per class).
This is guaranteed by `[ClassDataSource<TestApplication>]` which creates an `ApplicationFactory` with a unique SQLite in-memory database per instance.

## Unit tests are mandatory
Since the project is a library, development relies exclusively on unit tests.
