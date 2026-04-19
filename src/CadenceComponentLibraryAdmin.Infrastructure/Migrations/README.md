# Migrations

This folder is reserved for EF Core migrations.

The full entity model and DbContext for Milestone 2 are now in place.
Generate the initial migration with:

```powershell
dotnet ef migrations add InitialCreate --project src/CadenceComponentLibraryAdmin.Infrastructure --startup-project src/CadenceComponentLibraryAdmin.Web
dotnet ef database update --project src/CadenceComponentLibraryAdmin.Infrastructure --startup-project src/CadenceComponentLibraryAdmin.Web
```
