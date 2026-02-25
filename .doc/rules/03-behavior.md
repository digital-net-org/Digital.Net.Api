# Behavior rules

Strict rules to follow during any intervention on the codebase.

## Prohibitions

### No out-of-scope refactoring
**Never** refactor, rename, reorganize, or "improve" code that is not directly related to the current task.
If an issue is identified outside the current scope, report it without fixing it.

### No code comments
**Never** add comments in the code.
**Only exception**: XML documentation (`/// <summary>`, etc.) is allowed and encouraged on public APIs.

### No `.editorconfig` modifications
Do not modify the `.editorconfig` file unless explicitly asked.

### No unvalidated NuGet dependencies
Do not introduce new NuGet packages without prior validation.
Prefer existing internal solutions (e.g. `Mapper` instead of AutoMapper, `Result<T>` instead of FluentResults).
