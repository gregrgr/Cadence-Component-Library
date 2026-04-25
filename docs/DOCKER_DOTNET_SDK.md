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

## Codex CLI Docker bridge

The Docker Web container uses a dedicated `codex-cli` service for the `CodexCli` extraction provider. Do not call a host-installed `codex` executable from the Web container.

```powershell
docker compose --env-file .env.example -f docker-compose.yml up -d --build codex-cli
```

If Codex CLI authentication is required, use the bridge login helper from the host browser:

```text
http://localhost:4517/login
```

Click `Start login and open authentication page`. The helper starts `codex login` inside the `codex-cli` container and opens the authentication URL if the CLI prints one. The login state is stored in the Docker volume `codex-cli-home`.

If the browser helper cannot extract a login URL from the CLI output, fall back to an attached container login:

```powershell
docker compose --env-file .env.example -f docker-compose.yml run --rm --entrypoint codex codex-cli login
```

Then restart Web with the Docker bridge environment:

```powershell
$env:AI_EXTRACTION_MODE="CodexCli"
$env:AI_CODEXCLI_ENABLED="true"
$env:AI_CODEXCLI_TRANSPORT="HttpBridge"
$env:AI_CODEXCLI_BRIDGE_URL="http://codex-cli:4517"
$env:AI_CODEXCLI_PUBLIC_BRIDGE_URL="http://localhost:4517"
docker compose --env-file .env.example -f docker-compose.yml up -d --build web
```

After that, the `/AiIntake/{id}/RunExtraction` action will call the `codex-cli` container, and the container will invoke `codex exec`.

The service maps `127.0.0.1:4517` for the login helper and local health checks:

```powershell
curl.exe http://localhost:4517/health
```
