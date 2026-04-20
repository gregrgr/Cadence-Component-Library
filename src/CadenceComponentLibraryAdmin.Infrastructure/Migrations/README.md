# Migrations

This folder is reserved for EF Core migrations.

The current application still supports startup without generated migrations by using `DatabaseBootstrapper`.
That fallback remains in place for Milestone A audit and CI hardening.

The next infrastructure follow-up is to generate and commit the first formal migration:

```powershell
dotnet ef migrations add InitialCreate --project src/CadenceComponentLibraryAdmin.Infrastructure --startup-project src/CadenceComponentLibraryAdmin.Web
dotnet ef database update --project src/CadenceComponentLibraryAdmin.Infrastructure --startup-project src/CadenceComponentLibraryAdmin.Web
```
