# Architecture

Digital.Net is a **NuGet library** intended to be consumed by a .NET API application.
It provides a complete SDK (authentication, generic CRUD, document management, auditing, etc.)

## Modules

The system is separated into logical product modules. It is highly modular, allowing you to pick only the specific blocks you need for your application:

- **Digital.Net.Lib**: Foundational utilities and shared helpers used across all modules.
- **Digital.Net.Core.Entities**: EF Core models, DbContext, and entity definitions.
- **Digital.Net.Core**: The main API layer providing authentication, generic CRUD operations, endpoints, and services. Depends on `Digital.Net.Lib` and `Digital.Net.Core.Entities`.
- **Digital.Net.Cms**: A module that provides Content Management System capabilities on top of Core. Depends on `Digital.Net.Core`.
