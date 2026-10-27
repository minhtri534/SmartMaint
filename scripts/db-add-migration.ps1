# Adds an EF Core migration and regenerates database/Schema.sql in one step.
# Usage (repo root):  pwsh scripts/db-add-migration.ps1 AddBuildings
# Migration names: PascalCase, describe the change (AddBuildings, AddCaseStatusHistory) — never "Update1".
param([Parameter(Mandatory = $true)][string]$Name)
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
Push-Location (Join-Path $root 'backend')
try {
    dotnet tool restore | Out-Null
    dotnet ef migrations add $Name `
        --project src/SmartMaint.Api `
        --startup-project src/SmartMaint.Api `
        --output-dir Data/Migrations
}
finally {
    Pop-Location
}
& (Join-Path $PSScriptRoot 'db-schema.ps1')
