#!/usr/bin/env pwsh
# Record demo video of Gerador de Ofertas application
# Requires: ffmpeg installed and accessible

param(
    [ValidateSet('mp4', 'webm')]
    [string]$Format = 'mp4',

    [int]$Duration = 180,  # 3 minutes

    [string]$Output = './demo-video'
)

$ErrorActionPreference = 'Stop'

Write-Host "=== Gerador de Ofertas - Video Recording ===" -ForegroundColor Cyan
Write-Host "Format: $Format"
Write-Host "Duration: $Duration seconds"
Write-Host "Output: $Output.$Format"
Write-Host ""

# Check if ffmpeg is installed
try {
    $ffmpegVersion = ffmpeg -version 2>&1 | Select-Object -First 1
    Write-Host "✓ FFmpeg found: $ffmpegVersion" -ForegroundColor Green
} catch {
    Write-Host "✗ FFmpeg not found!" -ForegroundColor Red
    Write-Host ""
    Write-Host "Install FFmpeg:"
    Write-Host "  Windows: choco install ffmpeg"
    Write-Host "  Or download: https://ffmpeg.org/download.html"
    exit 1
}

Write-Host ""
Write-Host "IMPORTANT: Ensure both services are running:"
Write-Host "  1. Backend: cd app/backend && dotnet run"
Write-Host "  2. Frontend: cd app/frontend && npm start"
Write-Host ""
Write-Host "Record manually using:"
Write-Host "  - Windows 11 Game Bar (Win + G)"
Write-Host "  - OBS Studio (https://obsproject.com/)"
Write-Host "  - ScreenFlow (macOS)"
Write-Host "  - ffmpeg direct screen recording"
Write-Host ""

Write-Host "Recording will start in 5 seconds..." -ForegroundColor Yellow
Start-Sleep -Seconds 5

# Determine ffmpeg command based on OS
$isWindows = $PSVersionTable.Platform -eq 'Win32NT' -or $PSVersionTable.OS -like '*Windows*'

if ($isWindows) {
    # Windows screen recording (requires gdigrab)
    Write-Host "Starting screen recording (Windows)..." -ForegroundColor Cyan

    $codec = if ($Format -eq 'mp4') { 'libx264' } else { 'libvpx' }
    $ext = $Format

    ffmpeg -f gdigrab -framerate 30 -i desktop `
        -c:v $codec -preset medium -crf 23 `
        -pix_fmt yuv420p `
        -t $Duration `
        "$Output.$ext"
} else {
    # Linux/macOS recording
    Write-Host "Starting screen recording (macOS/Linux)..." -ForegroundColor Cyan

    # macOS uses avfoundation, Linux uses x11grab
    $input = if ($IsMacOS) { '1:0' } else { ':0.0' }
    $device = if ($IsMacOS) { 'avfoundation' } else { 'x11grab' }

    ffmpeg -f $device -framerate 30 -i $input `
        -c:v libx264 -preset medium -crf 23 `
        -pix_fmt yuv420p `
        -t $Duration `
        "$Output.$Format"
}

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "✓ Video recording complete!" -ForegroundColor Green
    Write-Host "Output: $Output.$Format"
    Write-Host ""
    Write-Host "File size:"
    if (Test-Path "$Output.$Format") {
        $size = (Get-Item "$Output.$Format").Length / 1MB
        Write-Host "  $([math]::Round($size, 2)) MB" -ForegroundColor Yellow
    }
} else {
    Write-Host "✗ Recording failed!" -ForegroundColor Red
    exit 1
}
