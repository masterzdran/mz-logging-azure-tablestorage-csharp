#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Build and pack NuGet packages for MasterZdran.Logging projects.

.DESCRIPTION
    This script builds the solution in Release mode and creates NuGet packages
    for both MasterZdran.Logging.AzureTableStorage and MasterZdran.Logging.Configuration projects.

.PARAMETER Configuration
    Build configuration (Debug or Release). Default: Release

.PARAMETER OutputPath
    Output directory for NuGet packages. Default: ./nupkgs

.PARAMETER SkipTests
    Skip running tests before packaging. Default: false

.PARAMETER Version
    Override the package version. If not specified, uses version from .csproj files.

.EXAMPLE
    .\build-nuget.ps1
    Build and pack with default settings (Release, run tests)

.EXAMPLE
    .\build-nuget.ps1 -Configuration Debug -SkipTests
    Build and pack in Debug mode without running tests

.EXAMPLE
    .\build-nuget.ps1 -Version 1.0.1
    Build and pack with version override
#>

[CmdletBinding()]
param(
    [Parameter()]
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release',

    [Parameter()]
    [string]$OutputPath = './nupkgs',

    [Parameter()]
    [switch]$SkipTests,

    [Parameter()]
    [string]$Version = $null
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# Colors for output
function Write-Header {
    param([string]$Message)
    Write-Host "`n=== $Message ===" -ForegroundColor Cyan
}

function Write-Success {
    param([string]$Message)
    Write-Host "✓ $Message" -ForegroundColor Green
}

function Write-Info {
    param([string]$Message)
    Write-Host "ℹ $Message" -ForegroundColor Yellow
}

function Write-ErrorMessage {
    param([string]$Message)
    Write-Host "✗ $Message" -ForegroundColor Red
}

# Get script directory
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $ScriptDir

Write-Header "MasterZdran.Logging NuGet Package Builder"

# Verify solution exists
if (-not (Test-Path "MasterZdran.Logging.sln")) {
    Write-ErrorMessage "Solution file not found: MasterZdran.Logging.sln"
    exit 1
}

# Clean previous builds
Write-Header "Cleaning Previous Builds"
if (Test-Path $OutputPath) {
    Remove-Item -Recurse -Force $OutputPath
    Write-Success "Cleaned output directory: $OutputPath"
}
New-Item -ItemType Directory -Force -Path $OutputPath | Out-Null

Write-Info "Cleaning solution..."
dotnet clean -c $Configuration --nologo
if ($LASTEXITCODE -ne 0) {
    Write-ErrorMessage "Clean failed"
    exit 1
}
Write-Success "Solution cleaned"

# Restore packages
Write-Header "Restoring NuGet Packages"
dotnet restore --nologo
if ($LASTEXITCODE -ne 0) {
    Write-ErrorMessage "Restore failed"
    exit 1
}
Write-Success "Packages restored"

# Build solution
Write-Header "Building Solution ($Configuration)"
dotnet build -c $Configuration --no-restore --nologo
if ($LASTEXITCODE -ne 0) {
    Write-ErrorMessage "Build failed"
    exit 1
}
Write-Success "Build completed successfully"

# Run tests
if (-not $SkipTests) {
    Write-Header "Running Tests"
    dotnet test -c $Configuration --no-build --nologo --verbosity minimal
    if ($LASTEXITCODE -ne 0) {
        Write-ErrorMessage "Tests failed"
        exit 1
    }
    Write-Success "All tests passed"
}
else {
    Write-Info "Skipping tests (--SkipTests specified)"
}

# Pack NuGet packages
Write-Header "Creating NuGet Packages"

$PackArgs = @(
    'pack'
    '-c', $Configuration
    '--no-build'
    '--nologo'
    '-o', $OutputPath
)

if ($Version) {
    $PackArgs += '/p:Version=' + $Version
    Write-Info "Using version override: $Version"
}

# Pack MasterZdran.Logging.AzureTableStorage
Write-Info "Packing MasterZdran.Logging.AzureTableStorage..."
$Project1 = "src/MasterZdran.Logging.AzureTableStorage/MasterZdran.Logging.AzureTableStorage.csproj"
& dotnet @PackArgs $Project1
if ($LASTEXITCODE -ne 0) {
    Write-ErrorMessage "Failed to pack MasterZdran.Logging.AzureTableStorage"
    exit 1
}
Write-Success "MasterZdran.Logging.AzureTableStorage packaged"

# Pack MasterZdran.Logging.Configuration
Write-Info "Packing MasterZdran.Logging.Configuration..."
$Project2 = "src/MasterZdran.Logging.Configuration/MasterZdran.Logging.Configuration.csproj"
& dotnet @PackArgs $Project2
if ($LASTEXITCODE -ne 0) {
    Write-ErrorMessage "Failed to pack MasterZdran.Logging.Configuration"
    exit 1
}
Write-Success "MasterZdran.Logging.Configuration packaged"

# Summary
Write-Header "Build Summary"

$Packages = Get-ChildItem -Path $OutputPath -Filter "*.nupkg" | Sort-Object Name
Write-Host ""
Write-Host "Created NuGet packages in $OutputPath :" -ForegroundColor Cyan
foreach ($Package in $Packages) {
    $SizeMB = [math]::Round($Package.Length / 1MB, 2)
    Write-Host "  📦 $($Package.Name) ($SizeMB MB)" -ForegroundColor White
}

Write-Host ""
Write-Success "NuGet packages created successfully!"
Write-Host ""
Write-Info "Next steps:"
Write-Host "  1. Test packages locally: dotnet add package <PackageName> --source $OutputPath"
Write-Host "  2. Publish to NuGet.org: dotnet nuget push $OutputPath\*.nupkg --api-key <key> --source https://api.nuget.org/v3/index.json"
Write-Host "  3. Publish to private feed: dotnet nuget push $OutputPath\*.nupkg --api-key <key> --source <feed-url>"
Write-Host ""
