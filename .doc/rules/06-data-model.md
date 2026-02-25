# Data Model

## Context

The database schema is entirely **driven by the application** via Entity Framework Core.
Models are located in `Digital.Net.Api.Entities/Models/`.

- **DbContext**: `DigitalContext`
- **DB schema**: `digital_net`
- **Production**: PostgreSQL
- **Tests**: SQLite (in-memory, one unique file per test)

## Entity hierarchy

```
IEntity (interface)
├── Id: Guid (auto-generated)
├── CreatedAt: DateTime
└── UpdatedAt: DateTime?

EntityMeta (abstract class)
├── CreatedAt [ReadOnly]
└── UpdatedAt [ReadOnly]

Entity : EntityMeta, IEntity (abstract class)
└── Id [Key, DatabaseGenerated]
```

All business entities inherit from `Entity`, which automatically provides:
- An auto-generated `Guid` identifier
- `CreatedAt` / `UpdatedAt` timestamps managed automatically by `Repository.AddTimestamps()`

## Entities

### User
| Property | Type | Constraints |
|----------|------|-------------|
| Username | `string` | Required, MaxLength(24), RegexValidation |
| Email | `string` | Required, MaxLength(254), RegexValidation |
| Login | `string` | Required, MaxLength(24), ReadOnly |
| Password | `string` | Required, MaxLength(128), Secret, ReadOnly |
| IsActive | `bool` | Required, ReadOnly |
| IsAdmin | `bool` | Required, ReadOnly |
| AvatarId | `Guid?` | FK → Avatar |
| Avatar | `Avatar?` | Navigation property |

Unique index: `(Username, Email)`

### Page
| Property | Type | Constraints |
|----------|------|-------------|
| Title | `string` | Required, MaxLength(64) |
| Description | `string` | MaxLength(256) |
| Path | `string` | Required, MaxLength(2068), Unique |
| IsPublished | `bool` | Required |
| IsIndexed | `bool` | Required |
| JsonLd | `string?` | DataFlag("json"), MaxLength(65535) |
| Metas | `List<PageOpenGraph>` | Navigation collection |

### Document
Represents an uploaded file.
- `UploaderId` → FK to `User`

### Avatar
Extension of `Document` for user avatars.

### Event
Audit event linked to a user.
- `UserId` → FK to `User`

### ApiKey
API key associated with a user.
- `UserId` → FK to `User`

### ApiToken
API token (refresh tokens) associated with a user.
- `UserId` → FK to `User`

### PageOpenGraph
Open Graph metadata associated with a page.

## Relationships

```
User ─────┬──── 1:N ──── ApiKey      (Cascade delete)
          ├──── 1:N ──── ApiToken    (Cascade delete)
          ├──── 1:N ──── Document    (Cascade delete)
          ├──── 1:N ──── Event       (Cascade delete)
          └──── N:1 ──── Avatar      (SetNull on delete)

Page ──────────── 1:N ──── PageOpenGraph
```

## Custom entity attributes

| Attribute | Role | Effect |
|-----------|------|--------|
| `[ReadOnly]` | Marks a field as non-modifiable | `CrudValidationService` rejects PATCH operations on these fields |
| `[Secret]` | Marks a field as sensitive | Excluded from schemas exposed via the API |
| `[RegexValidation(pattern)]` | Regex-based validation | Validated during CRUD validation |
| `[DataFlag(type)]` | Data type metadata | Informational (e.g. `"json"` for JSON stored as string) |

## Automatic timestamps

`Repository<T>` automatically manages timestamps:
- **Creation**: `CreatedAt` is populated with `DateTime.UtcNow`
- **Update**: `UpdatedAt` is populated with `DateTime.UtcNow`

This behavior is triggered in the repository's `Save()` / `SaveAsync()` methods via `ChangeTracker` inspection.
