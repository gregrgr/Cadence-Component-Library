# Audit Notes

This document captures the Milestone A hardening pass for buildability, smoke coverage, and runtime expectations.

## What was checked

- CI now runs `dotnet restore`, `dotnet build`, and `dotnet test` on every PR to `main`.
- CI provides a SQL Server service so the bootstrap smoke suite can execute the CIS view install path against a real relational database.
- The test project is no longer a placeholder. It includes executable smoke and unit coverage for:
  - identity seeding behavior
  - audit timestamps in `ApplicationDbContext`
  - package signature duplicate protection
  - approved-part validation rules
  - CIS view bootstrap statements
- Docker Compose is still the primary local runtime path for machines without a local `.NET 10 SDK`.
- `DatabaseBootstrapper` still owns schema bootstrapping and SQL view installation. The hardening pass did not replace the current architecture with a different startup model.
- The authoritative CIS view definition now comes from `src/CadenceComponentLibraryAdmin.Infrastructure/Data/Views/CisViews.sql`, which is copied into the application output for runtime bootstrap.

## Security and environment posture

- Roles are seeded in every environment.
- The default development administrator (`admin@local.test` / `Admin@123456`) is now seeded only when `ASPNETCORE_ENVIRONMENT=Development`.
- Non-development environments must provision their first administrator explicitly instead of receiving a known default password automatically.

## Current migration posture

- The application still supports startup without generated EF Core migrations by using the existing bootstrap fallback.
- A formal initial EF Core migration is still recommended as the next infrastructure-hardening step.
- The migration folder remains intentionally lightweight in this milestone so the audit/CI PR can focus on verifiable build, test, and runtime behavior without rewriting the data model.

## Manual smoke validation

Use this order on a machine with the `.NET 10 SDK` installed:

```powershell
git clone https://github.com/gregrgr/Cadence-Component-Library.git
cd Cadence-Component-Library
git checkout codex/audit-ci-hardening
dotnet restore
dotnet build
dotnet test
```

If SQL Server is not available locally, use Docker:

```powershell
Copy-Item .env.example .env
docker compose up --build
```

Expected development login:

- Email: `admin@local.test`
- Password: `Admin@123456`

## Milestone B0 Result

- A formal EF Core `InitialCreate` migration now exists and is committed.
- `dotnet ef database update` is the authoritative way to establish the SQL Server schema baseline.
- The initial migration creates the required application tables, ASP.NET Core Identity tables, and the CIS views.
- Runtime view refresh still uses `src/CadenceComponentLibraryAdmin.Infrastructure/Data/Views/CisViews.sql` through `DatabaseBootstrapper`.
- Database integration tests now verify:
  - the migration exists
  - SQL Server schema creation through migrations works
  - CIS views exist after migration
  - `PackageSignature` uniqueness is enforced by SQL Server
  - `Manufacturer + ManufacturerPN` uniqueness is enforced by SQL Server
- Remaining limitation:
  - non-development environments do not apply schema changes automatically unless `Database:ApplySchemaChangesOnStartup=true` is set intentionally.
