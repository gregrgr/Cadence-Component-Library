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

## Milestone B3 Result

- External staging import entities are implemented:
  - `ExternalImportSources`
  - `ExternalComponentImports`
  - `ExternalComponentAssets`
- `/ExternalImports` is implemented for authenticated review of staged imports and linked assets.
- EasyEDA import APIs are implemented under `/api/import/easyeda` and require `X-Import-Api-Key` for ingest.
- Imported records preserve normalized fields and raw JSON snapshots without auto-approving `CompanyParts`.
- Imported records can be converted explicitly into `OnlineCandidates`, keeping the workflow staging-only.
- Asset storage is file-based rather than SQL-blob based.
- The repository now includes an EasyEDA Pro extension project under `integrations/easyeda-pro-import-extension`.
- The import architecture and SDK capability scan are documented in `docs/EASYEDA_IMPORT.md`.
- .NET build and test validation pass on the hardened B3 branch.
- The EasyEDA extension now runs `typecheck`, sanity checks, and `build`, and CI validates that build path.
- Sample payloads are provided under `docs/samples/easyeda`.
- Asset upload validation covers configured storage-root use, SHA256 generation, and filename/path hardening.

Remaining limitations:

- EasyEDA Pro library APIs used by the extension are BETA and may change.
- `LIB_Device.getByLcscIds` may not work in private deployments.
- `LIB_Footprint.getRenderImage` is treated as best-effort because it was not confirmed through the public reference index used during implementation.
- STEP binaries may not always be obtainable directly from the SDK runtime, so URL preservation remains part of the design.

## Milestone B4 Result

- EasyEDA Pro SDK extension import is no longer the primary path.
- The backend now supports LCSC ID import through an nlbn-style EasyEDA API client.
- `/ExternalImports` exposes LCSC ID import and batch LCSC ID import flows.
- Raw EasyEDA response JSON, `dataStr`, `packageDetail`, `lcsc`, `c_para`, symbol shapes, and footprint shapes are preserved in staging.
- 3D `outline3D` metadata is extracted from footprint `SVGNODE` entries when present.
- Optional STEP / OBJ download is staged as external assets with SHA256.
- Raw symbol / footprint preservation is available directly from staged EasyEDA responses.
- Footprint preview generation is best-effort and falls back to a placeholder SVG when needed.
- Imported records still remain staging-only:
  - no automatic APPROVED `CompanyPart`
  - no automatic Released `FootprintVariant`
  - no automatic Allegro `PSM` / `DRA` conversion
- CI now focuses on `.NET` restore, build, and test; the legacy Node extension build is removed.
