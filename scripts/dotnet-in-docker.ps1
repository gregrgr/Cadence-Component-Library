param(
    [Parameter(ValueFromRemainingArguments = $true)]
    [string[]]$DotnetArgs
)

$repoRoot = Split-Path -Parent $PSScriptRoot
$envFile = if (Test-Path (Join-Path $repoRoot ".env")) { ".env" } else { ".env.example" }

if (-not $DotnetArgs -or $DotnetArgs.Count -eq 0) {
    Write-Error "Usage: ./scripts/dotnet-in-docker.ps1 <dotnet arguments>"
    exit 1
}

$originalLocation = Get-Location
Set-Location $repoRoot

$composeArgs = @(
    "compose",
    "--env-file", $envFile,
    "-f", "docker-compose.yml",
    "-f", "docker-compose.sdk.yml",
    "run",
    "--rm",
    "sdk",
    ('dotnet ' + ($DotnetArgs -join ' '))
)

& docker @composeArgs
$exitCode = $LASTEXITCODE
Set-Location $originalLocation
exit $exitCode
