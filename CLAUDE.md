# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Solution layout

This directory (`MoneyMonkey`) is the solution root. `MoneyMonkey.sln` lives here and wires together four project subfolders via project references:

- `MoneyMonkey.API` — ASP.NET Core 8 Web API host (Controllers, `Program.cs`, config, JWT/Swagger wiring). This is the startup/run project.
- `MoneyMonkey.Application` — services (business logic), JWT settings/token generation.
- `MoneyMonkey.Communication` — DTOs (`Request/`, `Response/`) and shared `Enums/`. No dependencies on other projects — this is the shared contract layer.
- `MoneyMonkey.Data` — EF Core `DbContext`, entities, and repositories (data access).

Dependency direction: `API -> Application -> Data -> Communication`, and `API`/`Data` also reference `Communication` directly. `Communication` has no project references.

There is no test project in the solution yet. This directory is not currently a git repository.

## Commands

Run from this directory (the solution root, where `MoneyMonkey.sln` lives):

```
dotnet build MoneyMonkey.sln              # build entire solution
dotnet run --project MoneyMonkey.API      # run the API
dotnet restore MoneyMonkey.sln
```

Launch profiles (`MoneyMonkey.API/Properties/launchSettings.json`) open Swagger UI at startup:
- `http` profile: http://localhost:5217/swagger
- `https` profile: https://localhost:7002/swagger (+ http://localhost:5217)

There is no EF Core migrations folder in `MoneyMonkey.Data` — the Postgres schema (tables `users`, `credentials`, `categories`, `transactions`, plus enum types `user_type`, `transaction_type`, `payment_method`) is expected to already exist in the target database; `OnModelCreating` in `MoneyMonkeyDbContext` maps to it but does not create it. If you add `dotnet-ef` migrations, confirm with the user first since this project currently manages schema out-of-band.

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

### Postgres enums

Three C# enums (`UserType`, `TransactionType`, `PaymentMethod` in `MoneyMonkey.Communication/Enums/`) map to native Postgres enum types. This mapping must be registered in **two places** that need to stay in sync when adding a new enum:
1. `Program.cs` — `NpgsqlDataSourceBuilder.MapEnum<T>("pg_type_name")` (for the `NpgsqlDataSource`) and again inside `UseNpgsql(...).MapEnum<T>(...)` (for the EF provider).
2. `MoneyMonkeyDbContext.OnModelCreating` — `modelBuilder.HasPostgresEnum<T>("public", "pg_type_name")` plus `.HasColumnType("pg_type_name")` on each property using it.

### Configuration

`appsettings.json` currently contains a plaintext local Postgres connection string and JWT signing secret checked into the file (dev defaults, `Host=localhost;...Password=1234`). Treat these as local-only placeholders, not real secrets.
