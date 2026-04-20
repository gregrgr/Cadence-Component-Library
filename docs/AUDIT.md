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

- The repository now has a formal EF Core baseline and follow-up migrations.
- `InitialCreate` remains the authoritative first schema baseline.
- `AddAdminAuditLogs` extends the baseline for admin-user and admin-role auditing without rewriting the existing architecture.

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

## Milestone B1 Result

- Approval Queue is implemented for pending `CompanyPart`, `OnlineCandidate`, and `FootprintVariant` review items.
- Alternates workflow is implemented with create, edit, approval, validation, and CompanyPart-detail integration.
- CompanyPart approval and alternate validation rules are enforced in application services and covered by tests.
- Remaining limitation:
  - workflow notifications and multi-step reviewer escalation are not implemented yet.

## Milestone B2 Result

- User administration is implemented at `/Admin/Users`.
- Role administration is implemented at `/Admin/Roles`.
- Authorization hardening is completed for the main mutation and approval paths:
  - Designer and Viewer cannot execute approval actions
  - PackageFamily and FootprintVariant mutations remain limited to Admin and Librarian
  - ManufacturerPart mutations remain limited to Admin, Librarian, and Purchasing
  - LibraryRelease remains Admin-only
- Menu visibility is now role-aware, but controller actions still enforce authorization server-side.
- Admin auditing now uses `AdminAuditLogs` for:
  - user creation
  - user lock and unlock
  - password reset
  - role changes
  - role creation, rename, and deletion
- Production first-admin bootstrap is available only through explicit configuration and is disabled by default.

Remaining limitations:

- Identity user records still rely on ASP.NET Core Identity defaults and do not currently expose created/updated timestamps.
- Bulk import/export, richer dashboard metrics, and deeper notification workflows remain out of scope for this milestone.
