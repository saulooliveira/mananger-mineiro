#!/usr/bin/env pwsh
# Build script for Gerador de Ofertas
# Generates single portable EXE with embedded .NET runtime

param(
    [ValidateSet('debug', 'release')]
    [string]$Configuration = 'release',

    [string]$OutputDir = './dist'
)

$ErrorActionPreference = 'Stop'

Write-Host "=== Gerador de Ofertas Build Pipeline ===" -ForegroundColor Cyan
Write-Host "Configuration: $Configuration" -ForegroundColor Yellow
Write-Host "Output: $OutputDir" -ForegroundColor Yellow
Write-Host ""

# Step 1: Build backend
Write-Host "[1/4] Building .NET backend (self-contained)..." -ForegroundColor Green
Push-Location app/backend
try {
    $backendConfig = if ($Configuration -eq 'release') { 'Release' } else { 'Debug' }
    dotnet publish -c $backendConfig -o ../frontend/public/backend --self-contained --no-restore
    if ($LASTEXITCODE -ne 0) {
        throw "Backend build failed"
    }
    Write-Host "✓ Backend published" -ForegroundColor Green
} finally {
    Pop-Location
}

# Step 2: Build frontend React
Write-Host "[2/4] Building React frontend..." -ForegroundColor Green
Push-Location app/frontend
try {
    npm run build
    if ($LASTEXITCODE -ne 0) {
        throw "Frontend build failed"
    }
    Write-Host "✓ Frontend built" -ForegroundColor Green
} finally {
    Pop-Location
}

# Step 3: Package with electron-builder
Write-Host "[3/4] Packaging with electron-builder (portable EXE)..." -ForegroundColor Green
Push-Location app/frontend
try {
    npm run dist
    if ($LASTEXITCODE -ne 0) {
        throw "Electron-builder packaging failed"
    }
    Write-Host "✓ Packaged to EXE" -ForegroundColor Green
} finally {
    Pop-Location
}

# Step 4: Copy output
Write-Host "[4/4] Finalizing output..." -ForegroundColor Green
if (-not (Test-Path $OutputDir)) {
    New-Item -ItemType Directory -Path $OutputDir | Out-Null
}

$exePath = "app/frontend/dist/*.exe"
$exeFiles = @(Get-ChildItem -Path $exePath -ErrorAction SilentlyContinue)

if ($exeFiles.Count -eq 0) {
    throw "No EXE files found in app/frontend/dist/"
}

foreach ($exe in $exeFiles) {
    Copy-Item -Path $exe.FullName -Destination $OutputDir -Force
    Write-Host "✓ Copied: $($exe.Name)" -ForegroundColor Green
}

Write-Host ""
Write-Host "=== Build Complete ===" -ForegroundColor Cyan
Write-Host "Output files:"
Get-ChildItem -Path $OutputDir -Filter "*.exe" | ForEach-Object {
    $sizeMB = [math]::Round($_.Length / 1MB, 2)
    Write-Host "  - $($_.Name) ($sizeMB MB)" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "✓ Single portable EXE ready to distribute" -ForegroundColor Green
