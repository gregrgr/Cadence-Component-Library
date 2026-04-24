# Dockerized .NET SDK

Use this path when the machine can run Docker but does not have a local `.NET 10 SDK`.

## Files

- `docker-compose.sdk.yml`
  - adds a reusable `sdk` service based on `mcr.microsoft.com/dotnet/sdk:10.0`
- `scripts/dotnet-in-docker.ps1`
  - wraps `docker compose ... run --rm sdk dotnet ...`

## First-time setup

```powershell
Copy-Item .env.example .env
docker compose --env-file .env -f docker-compose.yml -f docker-compose.sdk.yml up -d sqlserver
```

Notes:

- `sqlserver` is started first so `dotnet test` and `dotnet ef` can use the same SQL Server connection string as the web app.
- The SDK container mounts the repository at `/workspace`.
- NuGet packages are cached in a Docker-managed volume so repeated restores are faster.

## Common commands

```powershell
./scripts/dotnet-in-docker.ps1 restore
./scripts/dotnet-in-docker.ps1 build --no-restore --configuration Release
./scripts/dotnet-in-docker.ps1 test --no-build --configuration Release
```

## EF Core tooling

```powershell
./scripts/dotnet-in-docker.ps1 ef database update --project src/CadenceComponentLibraryAdmin.Infrastructure --startup-project src/CadenceComponentLibraryAdmin.Web
```

## Equivalent raw compose command

```powershell
docker compose --env-file .env -f docker-compose.yml -f docker-compose.sdk.yml run --rm sdk "dotnet restore"
```

The wrapper script is recommended because it handles the shell quoting required by the `sdk` container entrypoint.
