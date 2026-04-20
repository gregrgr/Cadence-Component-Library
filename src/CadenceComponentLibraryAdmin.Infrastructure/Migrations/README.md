# Migrations

Milestone B0 introduces the formal EF Core database baseline for this repository.

Current baseline:

- `InitialCreate` is the first committed EF Core migration.
- `dotnet ef database update` creates the SQL Server schema and the CIS release views.
- `DatabaseBootstrapper` is still used at runtime in Development to apply migrations automatically and refresh views from `Data/Views/CisViews.sql`.
- Non-development environments are expected to apply migrations explicitly before startup unless `Database:ApplySchemaChangesOnStartup=true` is set intentionally.

Common commands:

```powershell
dotnet ef migrations add <MigrationName> --project src/CadenceComponentLibraryAdmin.Infrastructure --startup-project src/CadenceComponentLibraryAdmin.Web
dotnet ef database update --project src/CadenceComponentLibraryAdmin.Infrastructure --startup-project src/CadenceComponentLibraryAdmin.Web
```
