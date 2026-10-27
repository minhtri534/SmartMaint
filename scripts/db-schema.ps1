# Regenerates database/Schema.sql from the EF Core migrations (idempotent script, safe to re-run).
# Run after every `dotnet ef migrations add ...` and commit Schema.sql together with the migration.
# Usage (repo root):  pwsh scripts/db-schema.ps1
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
Push-Location (Join-Path $root 'backend')
try {
    dotnet tool restore | Out-Null
    dotnet ef migrations script --idempotent `
        --project src/SmartMaint.Api `
        --startup-project src/SmartMaint.Api `
        --output (Join-Path $root 'database/Schema.sql')
    Write-Host 'database/Schema.sql regenerated.'
}
finally {
    Pop-Location
}
