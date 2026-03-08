# Architecture

Digital.Net.Api is a **NuGet library** intended to be consumed by a .NET API application.
It provides a complete SDK (authentication, generic CRUD, document management, auditing, etc.)

## Modules

The system is separated into logical product modules. It is highly modular, allowing you to pick only the specific blocks you need for your application:

- **Digital.Net.Api (Core)**: The foundational layer providing all the essential features, including authentication, generic CRUD operations, entities, and services. It is the heart of the library.
- **Digital.Net.Cms**: A module that provide Content Management System capabilities on top of the Core.
