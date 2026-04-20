# CadenceComponentLibraryAdmin

`CadenceComponentLibraryAdmin` is an ASP.NET Core MVC admin application for managing a Cadence / OrCAD component library baseline. It stores component master data in SQL Server, enforces core approval and packaging rules, and publishes CIS-facing read-only views for approved parts and alternates.

## Current scope

The repository currently includes:

- Layered solution structure:
  - `src/CadenceComponentLibraryAdmin.Web`
  - `src/CadenceComponentLibraryAdmin.Application`
  - `src/CadenceComponentLibraryAdmin.Domain`
  - `src/CadenceComponentLibraryAdmin.Infrastructure`
  - `tests/CadenceComponentLibraryAdmin.Tests`
- Core master-data modules:
  - `CompanyParts`
  - `ManufacturerParts`
  - `SymbolFamilies`
  - `PackageFamilies`
  - `FootprintVariants`
  - `OnlineCandidates`
- Workflow modules:
  - `Approval Queue`
  - `Alternates`
  - `Users / Roles` administration
  - `Change Logs`
  - `Quality Reports`
  - `Library Releases`
- Business rules:
  - package signature generation and duplicate prevention
  - approved part validation
  - approved-part footprint / symbol change logging
  - alternate-level checks
  - approval queue validation and status transitions
- Identity, role seeding, and admin bootstrap support
- Docker Compose runtime
- GitHub Actions CI
- Formal EF Core migration baseline (`InitialCreate`)

## Tech stack

- `.NET 10`
- `ASP.NET Core MVC`
- `Entity Framework Core`
- `SQL Server`
- `ASP.NET Core Identity`
- `Bootstrap 5`
- Docker Compose

## Repository layout

```text
CadenceComponentLibraryAdmin/
|- src/
|  |- CadenceComponentLibraryAdmin.Web/
|  |- CadenceComponentLibraryAdmin.Application/
|  |- CadenceComponentLibraryAdmin.Domain/
|  `- CadenceComponentLibraryAdmin.Infrastructure/
|- tests/
|  `- CadenceComponentLibraryAdmin.Tests/
|- docs/
|- library/
|- storage/
|- docker-compose.yml
|- .env.example
|- README.md
`- CadenceComponentLibraryAdmin.sln
```

## Roles and development admin

Roles seeded by the application:

- `Admin`
- `Librarian`
- `EEReviewer`
- `Purchasing`
- `Designer`
- `Viewer`

Development-only default admin:

- Email: `admin@local.test`
- Password: `Admin@123456`

Notes:

- The default admin is created only when `ASPNETCORE_ENVIRONMENT=Development`.
- Non-development environments still seed roles, but they do not create a known default administrator automatically unless the explicit bootstrap settings described below are enabled.

## Role permissions

Current role intent:

- `Admin`
  - full application access
  - can manage users, roles, approvals, releases, and library data
- `Librarian`
  - can edit library master data
  - can approve library-related workflow items
- `EEReviewer`
  - can review engineering-facing approval items
  - can review alternates and view library master data
- `Purchasing`
  - can manage manufacturer-part and AVL-related content
  - can view company parts and online candidates
- `Designer`
  - can submit and track online candidate requests
  - can view company parts
  - cannot perform approval actions
- `Viewer`
  - read-only access to approved/library-facing pages
  - cannot mutate package, footprint, manufacturer-part, or approval state

## Users and roles administration

Admin-only pages:

- `/Admin/Users`
  - search users by email or username
  - create users
  - edit user email, username, lockout settings, and role assignments
  - lock and unlock users
  - reset passwords
- `/Admin/Roles`
  - list roles and assigned users
  - create custom roles
  - rename custom roles only when unassigned
  - delete custom roles only when unassigned

Protection rules:

- seeded system roles cannot be renamed or deleted:
  - `Admin`
  - `Librarian`
  - `EEReviewer`
  - `Purchasing`
  - `Designer`
  - `Viewer`
- the current signed-in admin cannot remove their own `Admin` role
- the current signed-in admin cannot lock themselves
- at least one active `Admin` user must remain
- user and role admin actions are written to `AdminAuditLogs`

## Running locally

### Option 1: Docker Compose

This is the easiest path if the machine does not already have the `.NET 10 SDK`.

1. Copy the environment template:

```powershell
Copy-Item .env.example .env
```

2. Set a strong `SA_PASSWORD` in `.env`.

3. Start the stack:

```powershell
docker compose up --build
```

Runtime endpoints:

- Web: [http://localhost:8080](http://localhost:8080)
- SQL Server: `localhost:14333`

The default Docker flow uses `ASPNETCORE_ENVIRONMENT=Development`, so the web app can apply migrations and refresh CIS views automatically on startup.

### Option 2: Local .NET SDK

If `.NET 10 SDK` is installed:

```powershell
dotnet restore
dotnet build
dotnet test
dotnet ef database update --project src/CadenceComponentLibraryAdmin.Infrastructure --startup-project src/CadenceComponentLibraryAdmin.Web
dotnet run --project src/CadenceComponentLibraryAdmin.Web
```

## EF Core migration workflow

Milestone B0 establishes a formal EF Core baseline.

- `InitialCreate` is the first committed migration.
- `dotnet ef database update` is the authoritative schema-baseline command.
- The initial migration creates the SQL Server schema, ASP.NET Core Identity tables, and the baseline CIS views.

Create a new migration:

```powershell
dotnet ef migrations add <MigrationName> --project src/CadenceComponentLibraryAdmin.Infrastructure --startup-project src/CadenceComponentLibraryAdmin.Web
```

Apply migrations:

```powershell
dotnet ef database update --project src/CadenceComponentLibraryAdmin.Infrastructure --startup-project src/CadenceComponentLibraryAdmin.Web
```

Design-time connection string resolution:

- `ConnectionStrings__DefaultConnection`
- `CADENCE_DEFAULT_CONNECTION`

If neither is set, tooling falls back to the local trusted-connection default from the repository.

## Database startup behavior

- Development:
  - startup applies migrations automatically
  - startup refreshes CIS SQL views from the authoritative script
- Non-development:
  - startup does not apply schema changes unless `Database:ApplySchemaChangesOnStartup=true` is set intentionally
  - startup verifies that migrations are already applied and required CIS views already exist

This keeps development convenient while avoiding silent schema rewrites in higher environments.

## Production first-admin bootstrap

Recommended production pattern:

1. Deploy normally with role seeding enabled and `BootstrapAdmin:Enabled=false` (default).
2. When the first administrator is needed, set these environment variables temporarily for a single controlled startup:

```text
BootstrapAdmin__Enabled=true
BootstrapAdmin__Email=<admin email>
BootstrapAdmin__Password=<strong temporary password>
```

3. Start the application once, confirm the admin user can sign in, then remove or disable the bootstrap settings immediately.
4. Rotate the temporary bootstrap password after first use.

Important notes:

- There is no public unauthenticated admin-creation page.
- Bootstrap admin creation is disabled by default.
- Non-development environments should use bootstrap settings only for the initial admin or emergency recovery, then turn them off.

## CIS SQL views

Authoritative runtime view script:

- `src/CadenceComponentLibraryAdmin.Infrastructure/Data/Views/CisViews.sql`

Views:

- `dbo.vw_CIS_Release_Parts`
- `dbo.vw_CIS_Alternates`

Baseline behavior:

- the migration creates the views as part of the initial database baseline
- `DatabaseBootstrapper` can re-apply the script at runtime in Development so view changes stay deterministic during local iteration

## Docker persistence

The Docker stack persists:

- SQL Server data:
  - Docker volume `sqlserver-data`
- application data:
  - `./storage/app-data`
- library files:
  - `./library`
- application logs:
  - `./storage/logs`

The `library` directory is intended for Cadence assets such as:

- `Symbols_OLB`
- `Footprints`
- `Padstacks`
- `3D`
- `Docs`

## Testing and CI

GitHub Actions CI runs:

- `dotnet restore`
- `dotnet build --no-restore --configuration Release`
- `dotnet test --no-build --configuration Release`

CI also provides SQL Server so integration tests can verify:

- migration existence
- schema creation through EF Core migrations
- CIS view installation
- database-level unique constraints
- admin authorization and audit behavior

## Key database constraints and indexes

Important unique constraints:

- `CompanyPart.CompanyPN`
- `ManufacturerPart(Manufacturer, ManufacturerPN)`
- `SymbolFamily.SymbolFamilyCode`
- `PackageFamily.PackageFamilyCode`
- `PackageFamily.PackageSignature`
- `FootprintVariant.FootprintName`

Important indexes:

- `OnlineCandidate(Manufacturer, ManufacturerPN)`
- `CompanyPart.ApprovalStatus`
- `CompanyPart.DefaultFootprintName`
- `CompanyPart.PackageFamilyCode`

## Current limitations

- The repository still relies on SQL Server as the primary relational target; local work without SQL Server usually uses Docker.
- The baseline migration is formalized, but future schema changes still need deliberate migration discipline.
- The application still relies on ASP.NET Core Identity UI defaults for interactive sign-in and password policies.
- Automated vendor download, footprint generation, `.olb` generation, ERP / PLM sync, and advanced multi-step workflows are not implemented yet.

## Planned next steps

Recommended order after Milestone B2:

1. richer dashboard metrics
2. bulk import / export workflows
3. finer list filters and batch actions
4. deeper approval workflow and notification refinement
