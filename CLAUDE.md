# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Solution layout

This directory (`MoneyMonkey`) is the solution root. `MoneyMonkey.sln` lives here and wires together five project subfolders via project references:

- `MoneyMonkey.API` — ASP.NET Core 8 Web API host (Controllers, `Program.cs`, config, JWT/Swagger wiring). This is the startup/run project.
- `MoneyMonkey.Application` — services (business logic), JWT settings/token generation.
- `MoneyMonkey.Communication` — DTOs (`Request/`, `Response/`) and shared `Enums/`. No dependencies on other projects — this is the shared contract layer.
- `MoneyMonkey.Data` — EF Core `DbContext`, entities, repositories (data access), and EF Core `Migrations/`.
- `MoneyMonkey.Tests` — xUnit test project covering repositories and services.

Dependency direction: `API -> Application -> Data -> Communication`, and `API`/`Data` also reference `Communication` directly. `Communication` has no project references.

## Commands

Run from this directory (the solution root, where `MoneyMonkey.sln` lives):

```
dotnet tool restore                       # first time only: installs the dotnet-ef local tool from .config/dotnet-tools.json
dotnet build MoneyMonkey.sln              # build entire solution
dotnet run --project MoneyMonkey.API      # run the API (applies pending migrations automatically on startup)
dotnet restore MoneyMonkey.sln
dotnet test MoneyMonkey.sln               # run the MoneyMonkey.Tests suite
```

Launch profiles (`MoneyMonkey.API/Properties/launchSettings.json`) open Swagger UI at startup:
- `http` profile: http://localhost:5217/swagger
- `https` profile: https://localhost:7002/swagger (+ http://localhost:5217)

### Schema changes (EF Core Migrations)

The Postgres schema is owned by EF Core Migrations, not hand-written SQL. Migration files live in `MoneyMonkey.Data/Migrations/`, one file per table as a baseline (`CreateUsersTable`, `CreateCredentialsTable`, `CreateCategoriesTable`, `CreateTransactionsTable`) — keep that one-migration-per-logical-change granularity going forward instead of bundling unrelated table changes into a single migration.

To add a schema change:
```
dotnet dotnet-ef migrations add <Name> --project MoneyMonkey.Data --startup-project MoneyMonkey.API
```
This only generates a C# migration file (diffed against `MoneyMonkeyDbContext`'s current model) — it does not touch any database. To actually apply it to your local Postgres instance:
```
dotnet dotnet-ef database update --project MoneyMonkey.Data --startup-project MoneyMonkey.API
```
In the running API, `Program.cs` also calls `Database.Migrate()` once at startup (in every environment, not just Development) so pending migrations are applied automatically the next time the app runs — review generated SQL (`dotnet dotnet-ef migrations script`) before shipping anything schema-breaking, since a bad migration will crash startup.

Applied migrations are tracked in a table named **`Evolution`** (renamed from EF's default `__EFMigrationsHistory` via `npgsqlOptions.MigrationsHistoryTable("Evolution")` in `Program.cs`), not raw SQL run by hand against the database.

## Architecture

Standard layered flow per feature: **Controller → Service → Repository → `MoneyMonkeyDbContext`**, with DTOs (`Communication.Request`/`Communication.Response`) crossing every layer boundary instead of entities. Repositories return `Response` DTOs directly (they project with `.Select()` in the EF query) rather than returning entities to the service layer.

- **Controllers** (`MoneyMonkey.API/Controllers/`) are thin: extract the user id from the JWT (`ClaimTypes.NameIdentifier` via `User.FindFirstValue`), delegate to a service, and translate `null`/failure results into HTTP status codes (e.g. `BadRequest`/`Unauthorized`). All controllers except `AuthController` and `UserController.CreateUser`/`GetUsers`(mixed) are `[Authorize]`-protected.
- **Services** (`MoneyMonkey.Application/Services/`) are largely pass-through orchestrators over repositories today (e.g. `TransactionService`, `CategoryService`); `UserService` additionally coordinates login (authenticate + issue JWT via `ITokenService`).
- **Repositories** (`MoneyMonkey.Data/Repository/`) own all EF Core query/write logic and DTO projection. `UserRepository.CreateUser` wraps the two-step `User` + `Credential` insert in an explicit `Database.BeginTransactionAsync()` since they're separate tables/entities.
- **Multi-tenancy by convention**: nearly every query is scoped by `userId` (extracted from the JWT), e.g. `TransactionRepository`/`CategoryRepository` filter `Where(x => x.UserId == userId)`. When adding new queries/entities that belong to a user, follow this same scoping pattern — don't rely on global authorization alone.
- **Cross-entity validation happens in the repository**, not the DTO: e.g. `TransactionRepository.CreateTransaction` checks the referenced `CategoryId` belongs to the same `userId` before inserting, returning `null` (→ 400) if not.

### Auth

- Passwords are hashed with ASP.NET Identity's `IPasswordHasher<User>` (registered as a singleton) and stored in a separate `credentials` table (`Username`/`Password` keyed by `UserId`), decoupled from the `users` table.
- JWT issuing/validation config lives under the `Jwt` section in `appsettings.json` (`Secret`, `Issuer`, `Audience`, `ExpirationMinutes`), bound to `JwtSettings` (`MoneyMonkey.Application/Settings/JwtSettings.cs`). `TokenService` puts `UserId` in `sub`, full name in `ClaimTypes.Name`, and `UserType` in `ClaimTypes.Role`.

### Enums

Three C# enums (`UserType`, `TransactionType`, `PaymentMethod` in `MoneyMonkey.Communication/Enums/`) are stored as plain `text` columns, not native Postgres enum types — each is mapped with `.HasConversion<string>()` on the relevant property inside `MoneyMonkeyDbContext.OnModelCreating`. This is the only place the mapping is registered; `Program.cs` does not need any enum-specific wiring (no `NpgsqlDataSourceBuilder.MapEnum`/`HasPostgresEnum`). Adding a new enum value is a pure C# change — no migration required. Adding a brand-new enum still needs the usual `.HasConversion<string>()` line on its property plus a migration for the new/changed column.

### Configuration

`appsettings.json` currently contains a plaintext local Postgres connection string and JWT signing secret checked into the file (dev defaults, `Host=localhost;...Password=1234`). Treat these as local-only placeholders, not real secrets.
